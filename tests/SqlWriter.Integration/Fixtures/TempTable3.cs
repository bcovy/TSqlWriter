namespace SqlWriter.Integration.Fixtures;

[TableVariable("TableTmp3")]
public class TempTable3
{
    [ColumnPrecision(2, 1)]
    public decimal TaskNumber { get; set; }
}
