using System.Data;

namespace SqlWriter.Tests.Fixtures;

[TableName("Table4")]
public class QueryableMod4
{
    public int PropertyID { get; set; }
    public int Table4ID { get; set; }
    [ColumnSqlType(SqlDbType.VarChar)]
    public string Address { get; set; }
    [ColumnSqlType(SqlDbType.Date)]
    public DateTime? PcoeDate { get; set; }
    public int? TaskStatus { get; set; }
}