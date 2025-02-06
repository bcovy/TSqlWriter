namespace SqlWriter;
/// <summary>
/// Specifies the table name in the SQL database associated with entity.  Optional property to automagically identify 
/// entity's Primary Key field.
/// </summary>
/// <param name="name">Table name, including schema name.</param>
/// <param name="primaryKeyField">Primary key field name.</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class TableNameAttribute(string name, string primaryKeyField = "") : Attribute
{
    public string Name { get; private set; } = name;
    public string PrimaryKeyField { get; private set; } = primaryKeyField;
}