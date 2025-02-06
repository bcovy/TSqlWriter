using System.Data;

namespace SqlWriter.Tests.Fixtures;

[TableName("Table6")]
public class QueryableMod6
{
    public int PropertyID { get; set; }
    [ColumnSqlType(SqlDbType.VarChar, "VarChar (101)")]
    public string Address { get; set; }
    [ColumnSize(125)]
    public string FirstName { get; set; }
    [ColumnPrecision(3, 3)]
    public decimal Money { get; set; }
    [ColumnPrecision(4, 2)]
    public decimal? MoneyNullable { get; set; }
    public string EmptyString { get; set; }
    public decimal EmptyDec { get; set; }
    [ColumnSqlType(SqlDbType.NVarChar, "VarChar (101)")]
    [ColumnSize(125)]
    public string StringOverride { get; set; }
}
