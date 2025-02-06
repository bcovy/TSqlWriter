namespace SqlWriter.Tests.Fixtures;

[TableName("TaskTable")]
public class TaskMod
{
    public int EventID { get; set; }
    public string Comments { get; set; }
}
