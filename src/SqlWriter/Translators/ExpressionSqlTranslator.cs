using System.Linq.Expressions;
using System.Text;
using SqlWriter.Components.Parameters;
using SqlWriter.Components.Tables;
using SqlWriter.Infrastructure;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Translators;

public partial class ExpressionSqlTranslator : ExpressionVisitor, IExpressionSqlTranslator
{
    private StringBuilder _sb = null!;
    private bool _visitingBinary;  //Used to help identify target column of condition statement.
    private bool _addRawValue;  //If true, raw statement will be used.
    private bool _doNotParameterizeValues;  //If true, raw value will be used instead of parameter.
    private bool _excludeTableAlias;  //if true, will exclude the column's table alias.
    private string _parameterNamePrefix = "p";
    private readonly IParameterManager _parameterManager;

    private ITablesManager Tables { get; }
    public Stack<ColumnModel> Columns { get; private set; } = [];

    public ExpressionSqlTranslator(ITablesManager tables, IParameterManager parameterManager)
    {
        Tables = tables;
        _parameterManager = parameterManager;
    }

    public string Translate(Expression expression, string parameterNamePrefix = "p", bool doNotParameterizeValues = false)
    {
        _sb = new StringBuilder();
        _parameterNamePrefix = parameterNamePrefix;
        _doNotParameterizeValues = doNotParameterizeValues;
        Columns = new Stack<ColumnModel>();
        Visit(expression);
  
        _doNotParameterizeValues = false;

        return _sb.ToString();
    }

    public string TranslateWithoutAlias(Expression expression, string parameterNamePrefix = "p", bool doNotParameterizeValues = false)
    {
        _excludeTableAlias = true;
        _sb = new StringBuilder();
        _parameterNamePrefix = parameterNamePrefix;
        _doNotParameterizeValues = doNotParameterizeValues;
        Columns = new Stack<ColumnModel>();
        Visit(expression);

        _excludeTableAlias = false;
        _doNotParameterizeValues = false;

        return _sb.ToString();
    }

    #region Visitors
    protected override Expression VisitBinary(BinaryExpression node)
    {
        //Setting value to true marks the point in the branch that represents a single condition to be translated.
        _visitingBinary = true;

        //Get target column.
        Visit(node.Left);

        if (node.NodeType == ExpressionType.Equal && node.Right.IsNullUnaryOrConstant())
        {
            _sb.Append(" IS NULL");
        }
        else if (node.NodeType == ExpressionType.NotEqual && node.Right.IsNullUnaryOrConstant())
        {
            _sb.Append(" IS NOT NULL");
        }
        else
        {
            _sb.Append($" {BinaryLookup.Operation(node)} ");
            //Get value.
            Visit(node.Right);
        }

        _visitingBinary = false;

        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.BelongsToParameter())
        {
            CreateColumn(node);
        }
        else if (node.NodeType == ExpressionType.MemberAccess)
        {
            CreateParameter(node.GetValue());
        }

        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        CreateParameter(node.Value);

        return node;
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        switch (node.Operand)
        {
            case MemberExpression member:
                if (_visitingBinary && node.BelongsToParameter())
                    CreateColumn(member);
                else
                    Visit(member);
                break;
            case ConstantExpression constant:
                CreateParameter(constant.Value);
                break;
            case MethodCallExpression method:
                VisitMethodCall(method);
                break;
            case LambdaExpression lambda:
                Visit(lambda);
                break;
            case BinaryExpression binary:
                Visit(binary);
                break;
        }

        return node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // Complex switch statement has the best performance when benchmark-ing.  
        // Tried using dictionary of type <string, Action<ExpressionSqlTranslator, MethodCallExpression>
        // which produced results that were slightly worse.
        switch (node.Method.Name)
        {
            case "Average":
                ResolveAverage(node);
                break;
            case "Between":
                ResolveBetween(node);
                break;
            case "Cast":
                ResolveCast(node);
                break;
            case "Concat":
                _sb.Append(ConcatResolver.Resolve(node, Tables));
                break;
            case "Count":
                ResolveCount(node);
                break;
            case "CountOver":
                _sb.Append("COUNT(*) OVER()");
                break;
            case "DateDiff":
                ResolveDateDiff(node);
                break;
            case "EoMonth":
                ResolveEoMonth(node);
                break;
            case "Group":
                ResolveGroup(node);
                break;
            case "In":
            case "NotIn":
                CreateColumn(node.Arguments[0]);
                _sb.Append(ConditionInResolver.Resolve(node));
                break;
            case "IsNull":
                CreateColumn(node.Arguments[0]);
                _sb.Append(" IS NULL");
                break;
            case "IsNotNull":
                CreateColumn(node.Arguments[0]);
                _sb.Append(" IS NOT NULL");
                break;
            case "IIF":
                ResolveIif(node);
                break;
            case "Like":
                ResolveLike(node);
                break;
            case "Max":
            case "Min":
                ResolveMinMax(node);
                break;
            case "ParameterizedSql":
                ResolveParameterized(node);
                break;
            case "RawSql":
                ResolveRaw(node);
                break;
            case "Round":
                ResolveRound(node);
                break;
            case "Sum":
                ResolveSum(node);
                break;
        }

        return node;
    }

    #endregion Visitors 
    /// <summary>
    /// Appends target column name to string builder statement, and creates a <see cref="ColumnModel"/> object 
    /// to identify entity and its associated property/column used in statement.
    /// </summary>
    /// <param name="member">Column member expression.</param>
    private void CreateColumn(MemberExpression? member)
    {
        var table = Tables.GetTable(member);
        var column = new ColumnModel(member!.Member.Name, table.EntityType, table.TableAlias);

        if (_excludeTableAlias)
            _sb.Append(column.Name);
        else
            _sb.Append(column.TableAliasName).Append('.').Append(column.Name);

        Columns.Push(column);
    }
    
    private void CreateColumn(Expression? expression)
    {
        var table = Tables.GetTable(expression);
        var column = new ColumnModel(expression!.ResolveName(), table.EntityType, table.TableAlias);

        if (_excludeTableAlias)
            _sb.Append(column.Name);
        else
            _sb.Append(column.TableAliasName).Append('.').Append(column.Name);

        Columns.Push(column);
    }
    /// <summary>
    /// Adds parameter value to <see cref="ParameterManager"/>, and appends associated parameter name
    /// to string builder statement.  Will skip creating parameter and add raw value to string builder statement if
    /// <see cref="_addRawValue"/> or <see cref="_doNotParameterizeValues"/> is <see langword="true"/>
    /// </summary>
    /// <param name="value">Parameter value.</param>
    private void CreateParameter(object? value)
    {
        if (_addRawValue || _doNotParameterizeValues)
        {
            if (TypeHelper.IsNumeric(value))
            {
                _sb.Append(value);
            }
            else if (TypeHelper.IsDateTime(value))
            {
                _sb.Append($"'{value:yyyy-MM-dd HH:mm:ss}'");
            }
            else
            {
                _sb.Append($"'{value}'");
            }

            return;
        }

        string paramName = Columns.TryPeek(out var column1) 
            ? _parameterManager.Add(column1, value, _parameterNamePrefix) 
            : _parameterManager.Add(value, _parameterNamePrefix);

        _sb.Append(paramName);
    }
}