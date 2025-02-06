using System.Linq.Expressions;
using System.Text;
using SqlWriter.Compilers;
using SqlWriter.Components.Tables;
using SqlWriter.Components.Where;
using SqlWriter.Infrastructure;
using SqlWriter.Interfaces.Internals;
using SqlWriter.Translators;

namespace SqlWriter.Builders.Update;

public class UpdateBuilder<T> : BuilderBase, IUpdate<T> where T : class
{
    private readonly string _parameterPrefix;
    private readonly string _concatSql;

    private WhereBuilder WhereBuilder { get; }
    private TablesManager Tables { get; }
    private ExpressionSqlTranslator Translator { get; }
    public List<string> Columns { get; }

    public UpdateBuilder(IParameterManager parameterManager, string parameterPrefix = "p", string concatSqlStatement = "") 
        : base(parameterManager)
    {
        _parameterPrefix = parameterPrefix;
        _concatSql = concatSqlStatement;
        Tables = new TablesManager(typeof(T), "a");
        Translator = new ExpressionSqlTranslator(Tables, ParameterManager);
        WhereBuilder = new WhereBuilder(Translator, ParameterManager);
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

    private string BuildStatement()
    {
        var table = Tables.GetTable(typeof(T));
        string targets = string.Join(", ", Columns);
        StringBuilder sql = new StringBuilder("UPDATE ").Append($"{table.TableName} SET {targets}");

        if (WhereBuilder.HasConditions)
            sql.Append(WhereBuilder.Compile());

        return !string.IsNullOrEmpty(_concatSql) ? $"{_concatSql};\n{sql}" : sql.ToString();
    }

    public IUpdate<T> Set<TProperty>(Expression<Func<T, TProperty>> column, TProperty value)
    {
        var columnModel = Tables.GetColumn(typeof(T), column.ResolveName());
        string parameterName = ParameterManager.Add(columnModel, value, _parameterPrefix);

        Columns.Add($"{columnModel.Name} = {parameterName}");

        return this;
    }

    public IUpdate<T> Set<TProperty>(Expression<Func<T, TProperty>> column, Expression<Func<T, TProperty>> statement)
    {
        string sql = Translator.TranslateWithoutAlias(statement, _parameterPrefix);

        Columns.Add($"{column.ResolveName()} = {sql}");

        return this;
    }
    
    public IUpdate<T> SetNull<TProperty>(Expression<Func<T, TProperty>> column)
    {
        var columnModel = Tables.GetColumn(typeof(T), column.ResolveName());
        Columns.Add($"{columnModel.Name} = NULL");

        return this;
    }

    public IUpdate<T> SetRaw(string columnName, string columnValue)
    {
        Columns.Add($"{columnName} = {columnValue}");

        return this;
    }

    public IUpdate<T> SetRaw<TProperty>(Expression<Func<T, TProperty>> expression, string columnValue)
    {
        Columns.Add($"{expression.ResolveName()} = {columnValue}");

        return this;
    }

    public IUpdate<T> SetSubquery<TProperty>(Expression<Func<T, TProperty>> column, ISubquery subquery)
    {
        Columns.Add($"{column.ResolveName()} = {subquery.GetSqlStatement()}");
        //Add subquery parameters to current collection.
        ParameterManager.AddParameters(subquery.Parameters);

        return this;
    }

    public IUpdate<T> SetSubquery<TProperty>(Expression<Func<T, TProperty>> column, Func<ISubquery> subquery)
    {
        var compiled = subquery.Invoke();

        Columns.Add($"{column.ResolveName()} = {compiled.GetSqlStatement()}");
        //Add subquery parameters to current collection.
        ParameterManager.AddParameters(compiled.Parameters);

        return this;
    }

    public IUpdate<T> Where(Expression<Func<T, bool>> expression)
    {
        WhereBuilder.TranslateExpression(expression, _parameterPrefix, true);

        return this;
    }

    public IUpdate<T> WhereSubquery<TColumn>(Expression<Func<T, TColumn>> expression, Func<ISubquery> subquery)
    {
        WhereBuilder.WhereSubquery(expression, subquery, true);

        return this;
    }
}