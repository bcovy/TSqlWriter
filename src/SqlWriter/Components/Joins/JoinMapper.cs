using System.Linq.Expressions;
using SqlWriter.Interfaces;

namespace SqlWriter.Components.Joins;

public class JoinMapper : IJoinMapper
{
    public List<JoinMap> JoinMaps { get; } = [];

    public IJoinMapper Inner<TTable1, TTable2>() where TTable1 : class where TTable2 : class
    {
        JoinMaps.Add(new JoinMap(JoinType.Inner, typeof(TTable1), typeof(TTable2)) { UseEntity = true });

        return this;
    }

    public IJoinMapper Inner<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> joinExpression) where TTable1 : class where TTable2 : class
    {
        JoinMaps.Add(new JoinMap(JoinType.Inner, typeof(TTable1), typeof(TTable2)) { JoinExpression = joinExpression });

        return this;
    }

    public IJoinMapper Left<TTable1, TTable2>() where TTable1 : class where TTable2 : class
    {
        JoinMaps.Add(new JoinMap(JoinType.Left, typeof(TTable1), typeof(TTable2)) { UseEntity = true });

        return this;
    }

    public IJoinMapper Left<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> joinExpression) where TTable1 : class where TTable2 : class
    {
        JoinMaps.Add(new JoinMap(JoinType.Left, typeof(TTable1), typeof(TTable2)) { JoinExpression = joinExpression });

        return this;
    }

    public IJoinMapper Right<TTable1, TTable2>() where TTable1 : class where TTable2 : class
    {
        JoinMaps.Add(new JoinMap(JoinType.Right, typeof(TTable1), typeof(TTable2)) { UseEntity = true });

        return this;
    }

    public IJoinMapper Right<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> joinExpression) where TTable1 : class where TTable2 : class
    {
        JoinMaps.Add(new JoinMap(JoinType.Right, typeof(TTable1), typeof(TTable2)) { JoinExpression = joinExpression });

        return this;
    }
}