namespace SqlWriter.Tests.Fixtures;

[TableName("Table2", "PropertyID")]
public class QueryableMod2
{
    public int PropertyID { get; set; }
    public int? YesNo { get; set; }
    public short? DescID { get; set; }
    public int EventID { get; set; }
    public byte[] Rowversion { get; set; }
    public DateTime? PcoeDate { get; set; }
    public string Address { get; set; }
    public Guid Item { get; set; }
    public decimal DecimalVal { get; set; }
}