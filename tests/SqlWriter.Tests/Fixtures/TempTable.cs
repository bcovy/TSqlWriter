namespace SqlWriter.Tests.Fixtures;

[TableVariable("TableTmp")]
public class TempTable
{
    public int EventID { get; set; }
    [ColumnSize(25)]
    public string UserName { get; set; }
}
