using System.Data;
using System.Reflection;
using SqlWriter.Infrastructure;

namespace SqlWriter.Components.Tables;

public struct ColumnModel
{
    public string Name { get; }
    public Type TableType { get; }
    public string TableAliasName { get; }
    public SqlDbType SqlDataType { get; private set; }
    public string? TypeName { get; private set; }
    public int Size { get; private set; }
    public int Precision { get; private set; }
    public int Scale { get; private set; }

    public ColumnModel(string name, Type tableType, string tableAliasName)
    {
        Name = name;
        TableType = tableType;
        TableAliasName = tableAliasName;
        SetPropertiesFromAttributes(name);
    }

    private void SetPropertiesFromAttributes(string name)
    {
        var property = TableType.GetProperty(name);
        ArgumentNullException.ThrowIfNull(property);
        var sqlType = property.GetCustomAttribute<ColumnSqlTypeAttribute>();

        SqlDataType = sqlType?.DbType ?? property.TranslateSqlDbType();

        if (!string.IsNullOrWhiteSpace(sqlType?.TypeName))
        {
            TypeName = sqlType.TypeName;
            return;
        }

        if (property.PropertyType == typeof(string))
        {
            var size = property.GetCustomAttribute<ColumnSizeAttribute>();

            if (size == null) 
                return;
            
            Size = size.Size;
            TypeName = $"VARCHAR ({Size})";

            return;
        }

        if (property.PropertyType != typeof(decimal) &&
            Nullable.GetUnderlyingType(property.PropertyType) != typeof(decimal)) return;
        
        var precision = property.GetCustomAttribute<ColumnPrecisionAttribute>();

        if (precision == null) 
            return;
            
        Precision = precision.Precision;
        Scale = precision.Scale;
        TypeName = $"DECIMAL ({Precision}, {Scale})";
    }

    public override string ToString() => $"{TableAliasName}.{Name}";
}