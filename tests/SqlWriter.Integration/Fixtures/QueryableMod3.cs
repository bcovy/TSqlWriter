using System.Data;

namespace SqlWriter.Integration.Fixtures;

[TableName("Table3", "PropertyID")]
public class QueryableMod3
{
    public int PropertyID { get; set; }
    [ColumnSqlType(SqlDbType.VarChar)]
    public string Address { get; set; }
    [ColumnSqlType(SqlDbType.Date)]
    public DateOnly? PcoeDate { get; set; }
    public int? TaskStatus { get; set; }
    public decimal DecimalVal { get; set; }
    public decimal? DecimalValNull { get; set; }
}