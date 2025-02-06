namespace SqlWriter.Integration.Fixtures;

[TableVariable("TableTmp4")]
public class TempTable4
{
    [ColumnSize(10)]
    public string TaskNumber { get; set; }
}
