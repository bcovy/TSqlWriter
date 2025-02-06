namespace SqlWriter.Integration.Fixtures;

[TableName("Table5")]
public class QueryableMod5
{
    public int PropertyID { get; set; }
    public string Address { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateOnly? PcoeDate { get; set; }
    public DateOnly CloseDate { get; set; }
}