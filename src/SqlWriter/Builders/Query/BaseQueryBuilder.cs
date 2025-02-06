using System.Linq.Expressions;
using System.Text;
using Microsoft.Data.SqlClient;
using SqlWriter.Compilers;
using SqlWriter.Components.GroupBy;
using SqlWriter.Components.OrderBy;
using SqlWriter.Components.Select;
using SqlWriter.Components.Where;
using SqlWriter.Interfaces;
using SqlWriter.Interfaces.Internals;
using SqlWriter.Translators;

namespace SqlWriter.Builders.Query;

public abstract class BaseQueryBuilder
{
    private readonly ExpressionSqlTranslator _translator;
    private readonly string? _unionStatement;
    
    public string? HavingCondition { get; private set; }
    protected string ParameterPrefix { get; }
    protected ITablesManager Tables { get; }
    protected IParameterManager ParameterManager { get; }
    public SelectBuilder SelectBuilder { get; }
    public WhereBuilder WhereBuilder { get; }
    public GroupByBuilder GroupByBuilder { get; }
    public OrderByBuilder OrderByBuilder { get; }
    public IEnumerable<IParameterModel> Parameters => ParameterManager.Parameters;
    public SqlParameter[] GetSqlParameters => ParameterManager.GetSqlParameters;
    public IDictionary<string, object> GetParameters => ParameterManager.GetParameters;
    public List<(string Alias, string Column)>? CteColumns { get; private set; }
    private Dictionary<string, string>? CteStatements { get; set; }

    protected BaseQueryBuilder(ITablesManager tables, IParameterManager parameterManager, string parameterPrefix = "p", string? unionStatement = null)
    {
        _unionStatement = unionStatement;
        Tables = tables;
        ParameterManager = parameterManager;
        _translator = new ExpressionSqlTranslator(Tables, ParameterManager);
        SelectBuilder = new SelectBuilder(_translator, tables, ParameterManager);
        GroupByBuilder = new GroupByBuilder(Tables);
        OrderByBuilder = new OrderByBuilder(Tables);
        WhereBuilder = new WhereBuilder(_translator, ParameterManager);
        ParameterPrefix = parameterPrefix;
    }
    
    #region Compiler

    public IUnion Union()
    {
        if (CteColumns is not null)
            throw new NotSupportedException("Union operations are not supported for CTE statements at this time.");
        
        string statement = GetSqlStatement();
        return statement.EndsWith('\n') 
            ? new UnionSql(ParameterManager, $"{statement}UNION\n", ParameterPrefix) 
            : new UnionSql(ParameterManager, $"{statement}\nUNION\n", ParameterPrefix);
    }
    
    public IUnion UnionAll()
    {
        if (CteColumns is not null)
            throw new NotSupportedException("Union operations are not supported for CTE statements at this time.");
        
        string statement = GetSqlStatement();
        return statement.EndsWith('\n') 
            ? new UnionSql(ParameterManager, $"{statement}UNION ALL\n", ParameterPrefix) 
            : new UnionSql(ParameterManager, $"{statement}\nUNION ALL\n", ParameterPrefix);
    }
    
    public string GetSqlStatement()
    {
        StringBuilder sql = new();

        if (SelectBuilder.Columns.Count == 0)
            SelectBuilder.SelectAll();  //Apply select all as default if no columns were selected.

        if (CteColumns is not null)
        {
            sql.Append(CompileCte());
            //Add columns to parent Select statement.
            foreach (var (Alias, Column) in CteColumns)
                SelectBuilder.AddColumn(Column, $"{Alias}.{Column}");
        }

        sql.Append(SelectBuilder.Compile());
        sql.Append(Tables.Compile());

        if (WhereBuilder.HasConditions)
            sql.Append(WhereBuilder.Compile());

        if (GroupByBuilder.HasConditions)
            sql.Append(GroupByBuilder.Compile());

        if (!string.IsNullOrEmpty(HavingCondition))
            sql.Append($" HAVING {HavingCondition}");

        if (OrderByBuilder.HasConditions)
            sql.Append(OrderByBuilder.Compile());

        return !string.IsNullOrEmpty(_unionStatement) ? $"{_unionStatement}{sql}" : sql.ToString();
    }

    public string CompileCte()
    {
        StringBuilder sql = new("WITH ");

        foreach (var item in CteStatements!)
            sql.Append($"{item.Key} AS ({item.Value}), ").Append('\n');

        sql.Length -= 3; // remove last comma
        sql.AppendLine();

        return sql.ToString();
    }

    #endregion Compiler

    #region  Select

    public void SelectBase(params string[] columns)
    {
        foreach (string column in columns)
            SelectBuilder.AddColumn(column, column);
    }
    
    public void SelectBase(LambdaExpression columns)
    {
        SelectBuilder.TranslateExpression(columns, ParameterPrefix);
    }
    
    protected void SelectAllBase()
    {
        SelectBuilder.SelectAll();
    }

    protected void SelectProjectionBase<TProjection>() where TProjection : class
    {
        SelectBuilder.AddProjection<TProjection>();
    }

    protected void SelectTopBase(int topValue)
    {
        SelectBuilder.AddTopExpression(topValue);
    }

    protected void SelectRawBase(string statement, string aliasName)
    {
        SelectBuilder.AddColumn(aliasName, $"{statement} AS [{aliasName}]");
    }

    protected void SelectSubqueryBase(string columnName, ISubquery subquery)
    {
        SelectBuilder.AddColumn(columnName, $"{subquery.GetSqlStatement()} AS [{columnName}]");
        //Add subquery parameters to current collection.
        ParameterManager.AddParameters(subquery.Parameters);
    }

    public void SelectSubqueryBase(string columnName, Func<ISubquery> subquery)
    {
        var compiled = subquery.Invoke();
        
        SelectBuilder.AddColumn(columnName, $"{compiled.GetSqlStatement()} AS [{columnName}]");
        //Add subquery parameters to current collection.
        ParameterManager.AddParameters(compiled.Parameters);
    }

    #endregion
    
    #region Join

    protected void JoinWithMapper(Action<IJoinMapper> mapper)
    {
        Tables.JoinWithMapper(mapper);
    }

    protected void JoinBase<TTable1, TTable2>(JoinType joinType) where TTable1 : class where TTable2 : class
    {
        Tables.AddJoin<TTable1, TTable2>(joinType);
    }

    protected void JoinBase(LambdaExpression columns, JoinType joinType)
    {
        Tables.AddJoin(joinType, columns);
    }

    #endregion Join
    
    #region With CTE
    public void WithCte(LambdaExpression parentJoin, ICteStatement cteStatement, JoinType joinType = JoinType.Inner)
    {
        //make sure to call CompileStatement() first, so that select columns are identified.
        //Lazy load initial CTE statement.
        CteColumns ??= [];
        CteStatements ??= [];
        
        CteStatements.Add(cteStatement.CteAlias, cteStatement.CompileStatement());
        //Project CTE SELECT columns into parent query.
        if (parentJoin.Body is BinaryExpression binary)
        {
            var model = Tables.AddCteJoin(binary, joinType, cteStatement.CteAlias);

            if (!cteStatement.StopColumnProjection)
            {
                CteColumns.AddRange(cteStatement.IncludeJoinColumn
                    ? cteStatement.Columns.Select(s => (cteStatement.CteAlias, s))
                    : cteStatement.Columns.Where(w => w != model.ColumnRight && w != model.ColumnRight2)
                        .Select(s => (cteStatement.CteAlias, s)));
            }
        }
        else
        {
            string joinColumn = Tables.AddCteJoin(parentJoin, joinType, cteStatement.CteAlias);

            if (!cteStatement.StopColumnProjection)
            {
                CteColumns.AddRange(cteStatement.IncludeJoinColumn
                    ? cteStatement.Columns.Select(s => (cteStatement.CteAlias, s))
                    : cteStatement.Columns.Where(w => w != joinColumn)
                        .Select(s => (cteStatement.CteAlias, s)));
            }
        }

        if (cteStatement.Parameters.Any())
            ParameterManager.AddParameters(cteStatement.Parameters);
    }

    #endregion With CTE

    #region  Where

    public void WhereBase(LambdaExpression expression)
    {
        WhereBuilder.TranslateExpression(expression, ParameterPrefix);
    }
    
    public void WhereExistsBase<TExists, TTable>(Expression<Func<TExists, TTable, bool>> expression, bool isNotExist = false) where TExists : class where TTable : class
    {
        WhereExistsBuilder<TExists> existsBuilder = new(_translator, Tables, isNotExist);

        WhereBuilder.AddExist(existsBuilder.Compile(expression, ParameterPrefix));
    }
    
    protected void WhereSubqueryBase(LambdaExpression expression, Func<ISubquery> subquery)
    {
        var compiled = subquery.Invoke();

        WhereBuilder.WhereSubquery(expression, compiled);
    }

    protected void WhereSubqueryBase(LambdaExpression expression, ISubquery subquery)
    {
        WhereBuilder.WhereSubquery(expression, subquery);
    }

    #endregion
    
    #region Group by
    
    public void GroupByBase(LambdaExpression column)
    {
        GroupByBuilder.AddColumn(column);
    }

    public void HavingBase(LambdaExpression condition)
    {
        HavingCondition = _translator.Translate(condition, doNotParameterizeValues: true);
    }

    #endregion Group by

    #region Order by
    
    public void OrderByBase(LambdaExpression column, string direction)
    {
        OrderByBuilder.AddColumn(column, direction);
    }

    protected void OrderByBase(string column, string direction)
    {
        OrderByBuilder.AddColumn(column, direction);
    }

    protected void PagerBase(int pageIndex, int pageSize)
    {
        OrderByBuilder.AddPaging(pageIndex, pageSize);
    }

    #endregion Order by
}