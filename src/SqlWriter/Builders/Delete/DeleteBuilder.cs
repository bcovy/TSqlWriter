using System.Linq.Expressions;
using System.Text;
using SqlWriter.Components.Parameters;
using SqlWriter.Components.Tables;
using SqlWriter.Components.Where;
using SqlWriter.Translators;

namespace SqlWriter.Builders.Delete;

public class DeleteBuilder<T> : BuilderBase, IDelete<T> where T : class
{
    private readonly WhereBuilder _where;
    private readonly TablesManager _tables;

    public DeleteBuilder() : base(new ParameterManager())
    {
        _tables = new TablesManager(typeof(T), "a");
        _where = new WhereBuilder(new ExpressionSqlTranslator(_tables, ParameterManager), ParameterManager);
    }

    public string GetSqlStatement()
    {
        var table = _tables.GetTable(typeof(T));
        StringBuilder sql = new StringBuilder("DELETE ").Append(table.TableName);

        if (_where.HasConditions)
            sql.Append(_where.Compile());

        return sql.ToString();
    }

    public IDelete<T> Where(Expression<Func<T, bool>> expression)
    {
        _where.TranslateExpression(expression, excludeTableAlias: true);

        return this;
    }
}