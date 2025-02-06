using System.Data;

namespace SqlWriter.Tests.Fixtures;

[TableVariable("TableTmp2")]
public class TempTable2
{
    public int EventID { get; set; }
    [ColumnSqlType(SqlDbType.Int, "SMALLINT")]
    public int TaskNumber { get; set; }
}
