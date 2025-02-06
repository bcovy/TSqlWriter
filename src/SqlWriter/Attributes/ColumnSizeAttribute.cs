namespace SqlWriter;
/// <summary>
/// Defines the string size in bytes.  Only affects <see langword="string"/> column types.
/// </summary>
/// <remarks>
/// Attribute will be overridden in favor of <see cref="ColumnSqlTypeAttribute.TypeName"/> value.
/// </remarks>
/// <param name="size">String size in bytes.  Can be a value between 1 through 8000.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ColumnSizeAttribute(int size) : Attribute
{
    public int Size { get; } = size;
}
