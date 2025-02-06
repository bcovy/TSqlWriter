using System.Linq.Expressions;
using System.Text;
using SqlWriter.Components.Tables;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Components.Select;

public class SelectBuilder(IExpressionSqlTranslator translator, ITablesManager tables, IParameterManager parameterManager)
{
    private bool _hasTop;
    private int _topValue;

    public List<SelectColumn> Columns { get; } = [];

    public string Compile()
    {
        StringBuilder sql = new("SELECT ");

        if (_hasTop)
            sql.Append($"TOP ({_topValue}) ");

        string result = string.Join(", ", Columns.Select(x => x.ColumnValue));
        sql.Append(result);

        return sql.ToString();
    }

    public void SelectAll()
    {
        foreach (var item in tables.Tables.Select(x => x.Value.Columns))
        {
            var columns = item.Values.Select(x => new SelectColumn(x.Name, x.ToString()));

            Columns.AddRange(columns);
        }
    }

    public void TranslateExpression(LambdaExpression expression, string parameterNamePrefix = "sel")
    {
        if (expression.Body is not NewExpression newExp || newExp.Members is null)
            throw new TypeAccessException("Expected Select parameter type of NewExpression.");

        for (int index = 0; index < newExp.Arguments.Count; index++)
        {
            string memberName = newExp.Members[index].Name;
            //Handle raw values that don't have an associated database column.
            if (newExp.Arguments[index] is ConstantExpression constant)
            {
                string parameterName = parameterManager.Add(constant.Value, parameterNamePrefix);
                Columns.Add(new SelectColumn(memberName, $"{parameterName} AS [{memberName}]"));
            }
            else
            {
                string statement = translator.Translate(newExp.Arguments[index], parameterNamePrefix);
                //Statement is meant to make sure the last translated column statement is in fact a valid column name, rather than
                //a sql function with an alias name that matches the target column.  Example: EventID = SqlFunc.IIF(a.PropertyID == 1, 2, 3) AS [EventID]
                if (translator.Columns.TryPeek(out ColumnModel column) && column.Name == memberName && column.ToString() == statement)
                {
                    Columns.Add(new SelectColumn(column.Name, statement));
                }
                else
                {
                    Columns.Add(new SelectColumn(memberName, $"{statement} AS [{memberName}]"));
                }
            }
        }
    }

    public void AddColumn(string columnName, string columnValue) => Columns.Add(new SelectColumn(columnName, columnValue));

    public void AddTopExpression(int topValue)
    {
        _hasTop = true;
        _topValue = topValue;
    }
    /// <summary>
    /// Use <typeparamref name="TProjection"/> to project the columns that will appear in the SELECT statement.
    /// Column/property names in <typeparamref name="TProjection"/> must exactly match those in target entity(s).
    /// </summary>
    /// <typeparam name="TProjection">Projection entity.</typeparam>
    public void AddProjection<TProjection>() where TProjection : class
    {
        var model = Activator.CreateInstance<TProjection>();

        foreach (var property in model.GetType().GetProperties())
        {
            foreach (var item in tables.Tables)
            {
                if (!item.Value.Columns.TryGetValue(property.Name, out var result)) continue;
                
                Columns.Add(new SelectColumn(result.Name, result.ToString()));
                break;
            }
        }
    }
}