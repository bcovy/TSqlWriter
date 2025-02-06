using System.Data;

namespace SqlWriter.Integration.Fixtures;

[TableVariable("TableTmp2")]
public class TempTable2
{
    public int EventID { get; set; }
    [ColumnSqlType(SqlDbType.SmallInt)]
    public int TaskNumber { get; set; }
}
