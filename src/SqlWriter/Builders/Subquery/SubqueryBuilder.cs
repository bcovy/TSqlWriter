using System.Linq.Expressions;
using System.Text;
using SqlWriter.Components.Parameters;
using SqlWriter.Components.Tables;
using SqlWriter.Components.Where;
using SqlWriter.Infrastructure;
using SqlWriter.Translators;

namespace SqlWriter.Builders.Subquery;

public class SubqueryBuilder<TSub> : BuilderBase, ISubquery<TSub> where TSub : class
{
    private string _targetColumn = string.Empty;
    private readonly WhereBuilder _where;
    private readonly TablesManager _tables;
    private readonly ExpressionSqlTranslator _translator;

    public string ParameterPrefix { get; }
    public Prefix ConditionPrefix { get; }
    public Predicates ConditionPredicate { get; }

    public SubqueryBuilder(string parameterPrefix = "sub1", Predicates predicate = Predicates.Equal, Prefix prefix = Prefix.AND) 
        : base(new ParameterManager())
    {
        ParameterPrefix = parameterPrefix;
        ConditionPrefix = prefix;
        ConditionPredicate = predicate;
        _tables = new TablesManager(typeof(TSub), parameterPrefix);
        _translator = new ExpressionSqlTranslator(_tables, ParameterManager);
        _where = new WhereBuilder(_translator, ParameterManager);
    }

    public string GetSqlStatement()
    {
        var table = _tables.GetTable(typeof(TSub));

        StringBuilder builder = new StringBuilder("(SELECT ").Append($"{_targetColumn} FROM {table.TableName}");

        if (_where.HasConditions)
        {
            string where = _where.Compile();
            builder.Append($"{where}");
        }

        builder.Append(')');

        return builder.ToString();
    }

    public ISubquery<TSub> Select<T>(Expression<Func<TSub, T>> column)
    {
        _targetColumn = _translator.TranslateWithoutAlias(column, ParameterPrefix);

        return this;
    }

    public ISubquery<TSub> Count()
    {
        _targetColumn = "COUNT(*) AS [ColumnCount]";

        return this;
    }

    public ISubquery<TSub> Avg<T>(Expression<Func<TSub, T>> column)
    {
        _targetColumn = $"AVG({column.ResolveName()}) AS [AvgResult]";

        return this;
    }

    public ISubquery<TSub> Min<T>(Expression<Func<TSub, T>> column)
    {
        _targetColumn = $"MIN({column.ResolveName()}) AS [MinResult]";

        return this;
    }

    public ISubquery<TSub> Max<T>(Expression<Func<TSub, T>> column)
    {
        _targetColumn = $"MAX({column.ResolveName()}) AS [MaxResult]";

        return this;
    }

    public ISubquery<TSub> Raw(string statement)
    {
        _targetColumn = $"{statement} AS [ColumnRaw]";

        return this;
    }

    public ISubquery<TSub> Where(Expression<Func<TSub, bool>> expression)
    {
        _where.TranslateExpression(expression, ParameterPrefix, true);

        return this;
    }
}