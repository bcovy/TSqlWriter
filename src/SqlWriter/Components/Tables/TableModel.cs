using SqlWriter.Infrastructure;

namespace SqlWriter.Components.Tables;

public struct TableModel
{
    public string TableName { get; private set; }
    public string TableAlias { get; private set; }
    public bool HasPrimaryKeyField { get; private set; }
    public string PrimaryKeyField { get; }
    public Type EntityType { get; private set; }
    public Dictionary<string, ColumnModel> Columns { get; }
    public ColumnModel this[string key] => Columns[key];

    public TableModel(Type entityType, string alias)
    {
        var result = TableNameHelper.GetMetadata(entityType);

        EntityType = entityType;
        TableAlias = alias;
        TableName = result.Item1;
        PrimaryKeyField = result.Item2;
        HasPrimaryKeyField = !string.IsNullOrEmpty(PrimaryKeyField);
        Columns = [];

        foreach (string item in entityType.GetProperties().Select(x => x.Name))
        {
            Columns.Add(item, new ColumnModel(item, entityType, alias));
        }
    }
}