using System.Linq.Expressions;
using System.Text;
using Microsoft.Data.SqlClient;
using SqlWriter.Compilers;
using SqlWriter.Components.GroupBy;
using SqlWriter.Components.Select;
using SqlWriter.Components.Where;
using SqlWriter.Infrastructure;
using SqlWriter.Interfaces.Internals;
using SqlWriter.Translators;

namespace SqlWriter.Builders.Insert;

public abstract class BaseInsertBuilder
{
    private readonly string _insertTable;
    private readonly string _concatSql;
    private readonly Type _insertEntity;

    private bool _includeOutput;
    private string? _outputStatement;

    protected string? HavingCondition { get; set; }
    protected string ParameterPrefix { get; }
    protected ITablesManager Tables { get; }
    private IParameterManager ParameterManager { get; }
    public SelectBuilder SelectBuilder { get; }
    protected WhereBuilder WhereBuilder { get; }
    protected GroupByBuilder GroupByBuilder { get; }
    protected ExpressionSqlTranslator Translator { get; }
    public List<string> InsertTargets { get; private set; } = [];
    public IEnumerable<IParameterModel> Parameters => ParameterManager.Parameters;
    public SqlParameter[] GetSqlParameters => ParameterManager.GetSqlParameters;
    public IDictionary<string, object> GetParameters => ParameterManager.GetParameters;

    protected BaseInsertBuilder(ITablesManager tables, IParameterManager parameterManager, Type insertEntity, string parameterPrefix = "p", string concatSqlStatement = "")
    {
        _insertTable = TableNameHelper.GetName(insertEntity);
        _insertEntity = insertEntity;
        _concatSql = concatSqlStatement;
        Tables = tables;
        ParameterManager = parameterManager;
        Translator = new ExpressionSqlTranslator(Tables, ParameterManager);
        SelectBuilder = new SelectBuilder(Translator, tables, ParameterManager);
        WhereBuilder = new WhereBuilder(Translator, ParameterManager);
        GroupByBuilder = new GroupByBuilder(Tables);
        ParameterPrefix = parameterPrefix;
    }

    public string GetSqlStatement()
    {
        return BuildStatement();
    }

    public IConcatSql Concat()
    {
        return new ConcatSql(ParameterManager, BuildStatement(), ParameterPrefix);
    }

    public IConcatSql ConcatWithRowCount()
    {
        string sql = $"{BuildStatement()};\nIF @@ROWCOUNT = 0\nBEGIN     RETURN\nEND ";

        return new ConcatSql(ParameterManager, sql, ParameterPrefix);
    }

    public string BuildStatement()
    {
        if (InsertTargets.Count == 0)
        {
            //Use insert entity to derive insert column names and ordinal position.
            var projection = Activator.CreateInstance(_insertEntity, false);
            ArgumentNullException.ThrowIfNull(projection);
            InsertTargets = projection.GetType().GetProperties().Select(p => p.Name).ToList();
        }

        StringBuilder sql = new StringBuilder().Append($"INSERT INTO {_insertTable} (");
        //Build insert columns using key value.
        sql.Append(string.Join(", ", InsertTargets)).Append(")\n ");

        if (_includeOutput)
            sql.Append(_outputStatement).Append("\n ");

        sql.Append(SelectBuilder.Compile());
        sql.Append(Tables.Compile());

        if (WhereBuilder.HasConditions)
            sql.Append(WhereBuilder.Compile());

        if (GroupByBuilder.HasConditions)
            sql.Append(GroupByBuilder.Compile());

        if (!string.IsNullOrEmpty(HavingCondition))
            sql.Append($" HAVING {HavingCondition}");

        return !string.IsNullOrEmpty(_concatSql) ? $"{_concatSql};\n{sql}" : sql.ToString();
    }

    public void InsertColumnsFromProjection<TProjection>() where TProjection : class
    {
        TProjection projection = Activator.CreateInstance<TProjection>();

        InsertTargets = projection.GetType().GetProperties().Select(p => p.Name).ToList();
    }

    public void InsertColumnsFromExpression(LambdaExpression columns)
    {
        if (columns.Body is NewExpression { Members: not null } expNew)
            InsertTargets = expNew.Members.Select(x => x.Name).ToList();
        else
            throw new TypeAccessException("Expected Select parameter type of NewExpression.");
    }

    public void OutputTo<TInsert, TOutputTo>(Expression<Func<TInsert, TOutputTo>> columns) where TInsert : class where TOutputTo : class
    {
        OutputTranslator translator = new();

        _outputStatement = translator.Visit(columns);
        _includeOutput = true;
    }

    protected void AddWhereExists<TExists, TTable>(Expression<Func<TExists, TTable, bool>> expression, bool isNotExist = false) where TExists : class where TTable : class
    {
        WhereExistsBuilder<TExists> existsBuilder = new(Translator, Tables, isNotExist);

        WhereBuilder.AddExist(existsBuilder.Compile(expression, ParameterPrefix));
    }
}
