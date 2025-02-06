using SqlWriter.Interfaces.Internals;
using SqlWriter.Compilers;
using SqlWriter.Infrastructure;
using System.Linq.Expressions;

namespace SqlWriter.Builders.Insert;

public class InsertBuilder<T> : BuilderBase, IInsert<T> where T : class
{
    private readonly string _concatSql;
    private readonly string _parameterPrefix;
    private readonly ITablesManager _tables;
    
    public List<(string, string)> Columns { get; }

    public InsertBuilder(ITablesManager tables, IParameterManager parameterManager, string parameterPrefix = "p", string concatSqlStatement = "")
        : base(parameterManager)
    {
        _parameterPrefix = parameterPrefix;
        _concatSql = concatSqlStatement;
        _tables = tables;
        Columns = [];
    }

    public string GetSqlStatement()
    {
        return BuildStatement();
    }

    public IConcatSql Concat()
    {
        return new ConcatSql(ParameterManager, BuildStatement(), _parameterPrefix);
    }

    public IConcatSql ConcatWithRowCount()
    {
        string sql = $"{BuildStatement()};\nIF @@ROWCOUNT = 0\nBEGIN     RETURN\nEND ";

        return new ConcatSql(ParameterManager, sql, _parameterPrefix);
    }

    private string BuildStatement()
    {
        var table = _tables.GetTable(typeof(T));
        string targets = string.Join(", ", Columns.Select(x => x.Item1));
        string values = string.Join(", ", Columns.Select(x => x.Item2));
        string sql = $"INSERT INTO {table.TableName} ({targets}) VALUES ({values})";

        return !string.IsNullOrEmpty(_concatSql) ? $"{_concatSql};\n{sql}" : sql;
    }

    public IInsert<T> Set<TProperty>(Expression<Func<T, TProperty>> column, TProperty value)
    {
        var columnModel = _tables.GetColumn(typeof(T), column.ResolveName());
        string parameterName = ParameterManager.Add(columnModel, value, _parameterPrefix);

        Columns.Add((columnModel.Name, parameterName));

        return this;
    }

    public IInsert<T> SetRaw(string columnName, string columnValue)
    {
        Columns.Add((columnName, columnValue));

        return this;
    }

    public IInsert<T> SetRaw<TProperty>(Expression<Func<T, TProperty>> expression, string columnValue)
    {
        Columns.Add((expression.ResolveName(), columnValue));

        return this;
    }

    public IInsert<T> SetSubquery<TProperty>(Expression<Func<T, TProperty>> column, ISubquery subquery)
    {
        string name = column.ResolveName();

        Columns.Add((name, subquery.GetSqlStatement()));
        //Add subquery parameters to current collection.
        ParameterManager.AddParameters(subquery.Parameters);

        return this;
    }

    public IInsert<T> SetSubquery<TProperty>(Expression<Func<T, TProperty>> column, Func<ISubquery> subquery)
    {
        string name = column.ResolveName();
        var compiled = subquery.Invoke();

        Columns.Add((name, compiled.GetSqlStatement()));
        //Add subquery parameters to current collection.
        ParameterManager.AddParameters(compiled.Parameters);

        return this;
    }
}