using System.Linq.Expressions;
using SqlWriter.Components.Tables;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Components.Where;

public class WhereExistsBuilder<TExists> where TExists : class
{
    private readonly IExpressionSqlTranslator _translator;
    private readonly TableModel _existsTable;
    private readonly bool _isNotExists;

    public WhereExistsBuilder(IExpressionSqlTranslator translator, ITablesManager tablesManager, bool isNotExists = false)
    {
        //Check if table as been added to table manager.
        if (!tablesManager.ContainsEntity(typeof(TExists)))
            tablesManager.AddTable<TExists>("ext");

        _existsTable = tablesManager.GetTable(typeof(TExists));
        _translator = translator;
        _isNotExists = isNotExists;
    }

    public string Compile<TTable>(Expression<Func<TExists, TTable, bool>> expression, string parameterPrefix = "pw") where TTable : class
    {
        string statement = _translator.Translate(expression, parameterPrefix);

        return $"{(_isNotExists ? "NOT EXISTS" : "EXISTS")} (SELECT * FROM {_existsTable.TableName} AS {_existsTable.TableAlias} WHERE {statement})";
    }
}
