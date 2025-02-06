using System.Reflection;
using SqlWriter.Compilers;
using SqlWriter.Infrastructure;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Builders.TempTable;

public class TableVariableBuilderT2<T, T2> : BuilderBase, ITableVariable where T : class where T2 : class
{
    private readonly string _concatSql;
    private readonly string _parameterPrefix;

    private (string TableName, string[] Fields) Variable1 { get; }
    private (string TableName, string[] Fields) Variable2 { get; }

    public TableVariableBuilderT2(IParameterManager parameterManager, string parameterPrefix = "p", string concatSqlStatement = "")
        : base(parameterManager)
    {
        var table1 = typeof(T).GetCustomAttribute<TableVariableAttribute>()
            ?? throw new MissingFieldException($"The entity {typeof(T).Name} is missing the required TableVariable attribute.");
        var table2 = typeof(T2).GetCustomAttribute<TableVariableAttribute>()
            ?? throw new MissingFieldException($"The entity {typeof(T2).Name} is missing the required TableVariable attribute.");

        _parameterPrefix = parameterPrefix;
        _concatSql = concatSqlStatement;

        Variable1 = (table1.Name, SetFields(typeof(T)));
        Variable2 = (table2.Name, SetFields(typeof(T2)));
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
        string sql = $"DECLARE {Variable1.TableName} TABLE ({string.Join(", ", Variable1.Fields)});\nDECLARE {Variable2.TableName} TABLE ({string.Join(", ", Variable2.Fields)})";

        return !string.IsNullOrEmpty(_concatSql) ? $"{_concatSql};\n{sql}" : sql;
    }

    private static string[] SetFields(Type entityType)
    {
        List<string> fields = [];
        foreach (var property in entityType.GetProperties())
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
            //Fallback to sql type.
            fields.Add($"{property.Name} {sqlType?.DbType ?? property.TranslateSqlDbType()}");
        }

        return [.. fields];
    }
}
