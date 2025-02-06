namespace SqlWriter.Tests.Fixtures;

[TableName("TaskAssignments", "EventID")]
public class TaskAssignmentsMod
{
    public int EventID { get; set; }
    public int PropertyID { get; set; }
    public int TaskNumber { get; set; }
    public string Comments { get; set; }
}
