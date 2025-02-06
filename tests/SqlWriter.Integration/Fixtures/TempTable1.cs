using System.Data;

namespace SqlWriter.Integration.Fixtures;

[TableVariable("TableTmp")]
public class TempTable1
{
    public int EventID { get; set; }
    [ColumnSqlType(SqlDbType.VarChar, "VARCHAR(1000)")]
    public string UserName { get; set; }
}
