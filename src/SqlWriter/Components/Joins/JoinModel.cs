namespace SqlWriter.Components.Joins;

public class JoinModel(JoinType joinType, string tableName, string tableAlias)
{
    public JoinType JoinType { get; } = joinType;
    public string? ColumnLeft { get; set; }
    public string? ColumnRight { get; set; }
    public string? TableName { get; } = tableName;
    public string? TableAlias { get; } = tableAlias;
    public string? TargetTableAlias { get; set; }
    public bool IsCteJoin { get; init; }
    public bool IsCompositeJoin { get; set; }
    public bool IsCompositeJoinConstant { get; set; }
    public string? ColumnLeft2 { get; set; }
    public string? ColumnRight2 { get; set; }
    public string? CompositeTargetTableAlias { get; set; }
}