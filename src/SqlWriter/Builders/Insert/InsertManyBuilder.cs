using System.Linq.Expressions;
using System.Text;
using SqlWriter.Components.Parameters;
using SqlWriter.Components.Tables;
using SqlWriter.Infrastructure;

namespace SqlWriter.Builders.Insert;

/// <summary>
/// Builds a SQL insert statement for a single table with multiple insert values.
/// </summary>
/// <typeparam name="T">Entity table type.</typeparam>
public class InsertManyBuilder<T> : BuilderBase, IInsertMany<T> where T : class
{
    private readonly TableModel _tableModel;

    public List<string[]> InsertValues { get; } = [];
    public Dictionary<string, int> ColumnMapper { get; }

    public InsertManyBuilder(TableModel table) : base(new ParameterManager())
    {
        int index = 0;
        _tableModel = table;
        ColumnMapper = [];

        foreach (var item in _tableModel.Columns)
        {
            ColumnMapper[item.Key] = index;
            index++;
        }
    }

    public InsertManyBuilder(TableModel table, Expression<Func<T, object>> columns) : base(new ParameterManager())
    {
        _tableModel = table;
        ColumnMapper = [];

        if (columns.Body is NewExpression { Members: not null } targets)
        {
            for (int i = 0; i < targets.Arguments.Count; i++)
            {
                ColumnMapper[targets.Members[i].Name] = i;
            }
        }
        else
        {
            throw new TypeAccessException("Expected target columns parameter type of NewExpression.");
        }
    }

    public string GetSqlStatement()
    {
        string targets = string.Join(", ", ColumnMapper.Keys);
        StringBuilder sql = new StringBuilder().Append($"INSERT INTO {_tableModel.TableName} ({targets}) VALUES ").AppendLine();

        for (int i = 0; i < InsertValues.Count; i++)
        {
            sql.Append('(').Append(string.Join(", ", InsertValues[i])).Append(')');

            if (i + 1 < InsertValues.Count)
                sql.AppendLine().Append(" ,");
        }

        return sql.ToString();
    }

    public IInsertMany<T> SetValues(T entity)
    {
        string[] values = new string[ColumnMapper.Count];

        foreach (var item in ColumnMapper)
        {
            if (entity is null)
            {
                values[item.Value] = "NULL";
                continue;
            }
            
            var result = entity.GetType().GetProperty(item.Key)?.GetValue(entity, null);

            if (result == null)
            {
                values[item.Value] = "NULL";
            }
            else if (TypeHelper.IsNumeric(result))
            {
                values[item.Value] = result.ToString() ?? "NULL";
            }
            else 
            {
                values[item.Value] = $"'{result}'";
            }
        }

        InsertValues.Add(values);

        return this;
    }
}