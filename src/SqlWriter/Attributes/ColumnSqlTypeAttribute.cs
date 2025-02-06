using System.Data;

namespace SqlWriter;
/// <summary>
/// Specifies SQL server data type of the property.
/// </summary>
/// <param name="dbType">Specifies SQL server data type.</param>
/// <param name="typeName">Defines an exact data type.  Example: VARCHAR(200).</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ColumnSqlTypeAttribute(SqlDbType dbType, string typeName = "") : Attribute
{
    public SqlDbType DbType { get; private set; } = dbType;
    public string TypeName { get; private set; } = typeName;
}