using System.Reflection;
using SqlWriter.Compilers;
using SqlWriter.Infrastructure;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Builders.TempTable;

public class TableVariableBuilder : BuilderBase, ITableVariable
{
    private readonly string _concatSql;
    private readonly string _tableName;
    private readonly string _parameterPrefix;

    private Type EntityType { get; }
    public string[] Fields { get; }

    public TableVariableBuilder(Type entityType, IParameterManager parameterManager, string parameterPrefix = "p", string concatSqlStatement = "")
        : base(parameterManager)
    {
        var table = entityType.GetCustomAttribute<TableVariableAttribute>()
            ?? throw new MissingFieldException($"The entity {entityType.Name} is missing the required TableVariable attribute.");

        _parameterPrefix = parameterPrefix;
        _concatSql = concatSqlStatement;
        _tableName = table.Name;
        EntityType = entityType;
        Fields = SetFields();
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
        string sql = $"DECLARE {_tableName} TABLE ({string.Join(", ", Fields)})";

        return !string.IsNullOrEmpty(_concatSql) ? $"{_concatSql};\n{sql}" : sql;
    }

    private string[] SetFields()
    {
        List<string> fields = [];
        foreach (var property in EntityType.GetProperties())
        {
            var sqlType = property.GetCustomAttribute<ColumnSqlTypeAttribute>();

            if (!string.IsNullOrWhiteSpace(sqlType?.TypeName))
            {
                fields.Add($"{property.Name} {sqlType.TypeName}");
                continue;
            }

            if (property.PropertyType == typeof(string))
            {
                var size = property.GetCustomAttribute<ColumnSizeAttribute>();

                if (size != null)
                    fields.Add($"{property.Name} VARCHAR ({size.Size})");

                continue;
            }

            if (property.PropertyType == typeof(decimal) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(decimal))
            {
                var precision = property.GetCustomAttribute<ColumnPrecisionAttribute>();

                if (precision != null)
                    fields.Add($"{property.Name} DECIMAL ({precision.Precision}, {precision.Scale})");

                continue;
            }
            //Fallback to sqltype
            fields.Add($"{property.Name} {sqlType?.DbType ?? property.TranslateSqlDbType()}");
        }

        return [.. fields];
    }
}
