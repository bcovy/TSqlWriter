using System.Linq.Expressions;
using System.Text;
using SqlWriter.Compilers;
using SqlWriter.Components.Where;
using SqlWriter.Infrastructure;
using SqlWriter.Interfaces.Internals;
using SqlWriter.Translators;

namespace SqlWriter.Builders.Update;

public class UpdateBuilderT<TUpdate, TSelect> : BuilderBase, IUpdate<TUpdate, TSelect> where TUpdate : class where TSelect : class
{
    private readonly string _parameterPrefix;
    private readonly string _concatSql;

    private bool _includeOutput;
    private string? _outputStatement;

    private WhereBuilder WhereBuilder { get; }
    private ITablesManager Tables { get; }
    private ExpressionSqlTranslator Translator { get; }
    public List<string> Columns { get; }

    public UpdateBuilderT(ITablesManager tables, IParameterManager parameterManager, string parameterPrefix = "p", string concatStatement = "")
        : base(parameterManager)
    {
        _parameterPrefix = parameterPrefix;
        _concatSql = concatStatement;
        Tables = tables;
        Translator = new ExpressionSqlTranslator(Tables, parameterManager);
        WhereBuilder = new WhereBuilder(Translator, parameterManager);
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
        var table = Tables.GetTable(typeof(TUpdate));
        string targets = string.Join(", ", Columns);
        StringBuilder sql = new StringBuilder("UPDATE ").Append($"{table.TableName} SET {targets}");

        if (_includeOutput)
            sql.Append('\n').Append(_outputStatement);
        //Add table join statements.
        sql.AppendLine().Append(Tables.Compile());

        if (WhereBuilder.HasConditions)
            sql.Append(WhereBuilder.Compile());

        return !string.IsNullOrEmpty(_concatSql) ? $"{_concatSql};\n{sql}" : sql.ToString();
    }

    #region Set
    public IUpdate<TUpdate, TSelect> Set<TProperty>(Expression<Func<TUpdate, TProperty>> column, TProperty value)
    {
        string name = column.ResolveName();
        var columnModel = Tables.GetColumn(typeof(TUpdate), name);
        string parameterName = ParameterManager.Add(columnModel, value, _parameterPrefix);

        Columns.Add($"{name} = {parameterName}");

        return this;
    }

    public IUpdate<TUpdate, TSelect> Set<T, T2>(Expression<Func<TUpdate, T>> column, Expression<Func<TSelect, T2>> statement)
    {
        string sql = Translator.Translate(statement, _parameterPrefix);

        Columns.Add($"{column.ResolveName()} = {sql}");

        return this;
    }
    
    public IUpdate<TUpdate, TSelect> SetNull<TProperty>(Expression<Func<TUpdate, TProperty>> column)
    {
        var columnModel = Tables.GetColumn(typeof(TUpdate), column.ResolveName());
        Columns.Add($"{columnModel.Name} = NULL");

        return this;
    }

    public IUpdate<TUpdate, TSelect> SetRaw(string columnName, string columnValue)
    {
        Columns.Add($"{columnName} = {columnValue}");

        return this;
    }

    public IUpdate<TUpdate, TSelect> SetRaw<TProperty>(Expression<Func<TUpdate, TProperty>> expression, string columnValue)
    {
        Columns.Add($"{expression.ResolveName()} = {columnValue}");

        return this;
    }

    #endregion Set

    #region Output
    public IUpdate<TUpdate, TSelect> OutputTo<TOutputTo>(Expression<Func<TUpdate, UpdateOutput<TOutputTo>>> statement) where TOutputTo : class
    {
        OutputTranslator translator = new();

        _outputStatement = translator.Visit(statement);
        _includeOutput = true;

        return this;
    }

    #endregion Output

    #region Where
    public IUpdate<TUpdate, TSelect> Where(Expression<Func<TUpdate, TSelect, bool>> expression)
    {
        WhereBuilder.TranslateExpression(expression, _parameterPrefix);

        return this;
    }

    public IUpdate<TUpdate, TSelect> WhereExists<TExists>(Expression<Func<TExists, TSelect, bool>> expression) where TExists : class
    {
        WhereExistsBuilder<TExists> existsBuilder = new(Translator, Tables);

        WhereBuilder.AddExist(existsBuilder.Compile(expression, _parameterPrefix));

        return this;
    }

    public IUpdate<TUpdate, TSelect> WhereNotExists<TExists>(Expression<Func<TExists, TSelect, bool>> expression) where TExists : class
    {
        WhereExistsBuilder<TExists> existsBuilder = new(Translator, Tables, true);

        WhereBuilder.AddExist(existsBuilder.Compile(expression, _parameterPrefix));

        return this;
    }

    public IUpdate<TUpdate, TSelect> WhereSubquery<TColumn>(Expression<Func<TUpdate, TColumn>> expression, Func<ISubquery> subquery)
    {
        WhereBuilder.WhereSubquery(expression, subquery);

        return this;
    }

    #endregion Where
}