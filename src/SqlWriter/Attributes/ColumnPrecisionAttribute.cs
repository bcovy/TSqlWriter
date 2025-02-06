namespace SqlWriter;
/// <summary>
/// Sets the fixed precision and scale for <see langword="decimal"/> data types.
/// </summary>
/// <remarks>
/// Attribute will be overridden in favor of <see cref="ColumnSqlTypeAttribute.TypeName"/> value.
/// </remarks>
/// <param name="precision">Maximum total number of decimal digits to be stored.</param>
/// <param name="scale">Number of decimal digits that are stored to the right of the decimal point.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ColumnPrecisionAttribute(int precision, int scale) : Attribute
{
    public int Precision { get; } = precision;
    public int Scale { get; } = scale;
}
