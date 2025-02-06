using System.Linq.Expressions;

namespace SqlWriter.Components.Joins;

public class JoinMap(JoinType joinType, Type table1, Type table2)
{
    public bool UseEntity { get; init; }
    public Type Table1 { get; } = table1;
    public Type Table2 { get; } = table2;
    public JoinType JoinType { get; } = joinType;
    public LambdaExpression? JoinExpression { get; init; }
}