namespace SqlWriter.Integration.Fixtures;

[TableName("Table1", "PropertyID")]
public class QueryableMod1
{
    public int PropertyID { get; set; }
    public string Address { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime? PcoeDate { get; set; }
    public byte[] Rowversion { get; set; }
}