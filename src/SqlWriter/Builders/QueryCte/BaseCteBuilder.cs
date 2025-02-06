using System.Text;
using SqlWriter.Components.GroupBy;
using SqlWriter.Components.OrderBy;
using SqlWriter.Components.Parameters;
using SqlWriter.Components.Select;
using SqlWriter.Components.Where;
using SqlWriter.Interfaces.Internals;
using SqlWriter.Translators;

namespace SqlWriter.Builders.QueryCte;

public abstract class BaseCteBuilder
{
    private readonly ParameterManager _parameterManager;
    
    protected string? HavingCondition { get; set; }
    protected string ParameterPrefix { get; }
    protected ITablesManager Tables { get; }
    protected SelectBuilder SelectBuilder { get; }
    protected WhereBuilder WhereBuilder { get; }
    protected GroupByBuilder GroupByBuilder { get; }
    protected OrderByBuilder OrderByBuilder { get; }
    protected ExpressionSqlTranslator Translator { get; }
    public string CteAlias { get; }
    public bool StopColumnProjection { get; }
    public bool IncludeJoinColumn { get; }
    public IEnumerable<string> Columns { get; private set; } = [];
    public IEnumerable<IParameterModel> Parameters => _parameterManager.Parameters;

    protected BaseCteBuilder(ITablesManager tables, string cteAlias, bool stopColumnProjection, bool includeJoinColumn = false, string parameterPrefix = "p")
    {
        Tables = tables;
        _parameterManager = new ParameterManager();
        Translator = new ExpressionSqlTranslator(Tables, _parameterManager);
        SelectBuilder = new SelectBuilder(Translator, Tables, _parameterManager);
        GroupByBuilder = new GroupByBuilder(Tables);
        OrderByBuilder = new OrderByBuilder(Tables);
        WhereBuilder = new WhereBuilder(Translator, _parameterManager);
        ParameterPrefix = parameterPrefix;
        CteAlias = cteAlias;
        StopColumnProjection = stopColumnProjection;
        IncludeJoinColumn = includeJoinColumn;
    }

    public string CompileStatement()
    {
        StringBuilder sql = new();

        if (SelectBuilder.Columns.Count == 0)
            SelectBuilder.SelectAll();  //Apply select all as default if no columns were selected.

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

        Columns = SelectBuilder.Columns.Select(x => x.ColumnName);

        return sql.ToString();
    }
}
