using System.Linq.Expressions;
using SqlWriter.Components.Joins;
using SqlWriter.Tests.Fixtures;

namespace SqlWriter.Tests.Components;

public class JoinMapperTest
{
    private readonly JoinMapper _feature;

    public JoinMapperTest()
    {
        _feature = new JoinMapper();
    }

    [Fact]
    public void Inner_should_add_join_statement_to_map_list()
    {
        Expression<Func<QueryableMod2, QueryableMod3, bool>> join = (a, b) => a.PropertyID == b.PropertyID;

        _feature.Inner(join);

        var actual = Assert.Single(_feature.JoinMaps);
        Assert.Equal(JoinType.Inner, actual.JoinType);
    }

    [Fact]
    public void Left_should_add_join_statement_to_map_list()
    {
        Expression<Func<QueryableMod2, QueryableMod3, bool>> join = (a, b) => a.PropertyID == b.PropertyID;

        _feature.Left(join);

        var actual = Assert.Single(_feature.JoinMaps);
        Assert.Equal(JoinType.Left, actual.JoinType);
    }

    [Fact]
    public void Right_should_add_join_statement_to_map_list()
    {
        Expression<Func<QueryableMod2, QueryableMod3, bool>> join = (a, b) => a.PropertyID == b.PropertyID;

        _feature.Right(join);

        var actual = Assert.Single(_feature.JoinMaps);
        Assert.Equal(JoinType.Right, actual.JoinType);
    }
}