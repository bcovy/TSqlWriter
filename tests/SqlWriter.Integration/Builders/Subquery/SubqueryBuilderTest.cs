using SqlWriter.Builders.Subquery;
using SqlWriter.Integration.Fixtures;

namespace SqlWriter.Integration.Builders.Subquery;

public class SubqueryBuilderTest
{
    private readonly SubqueryBuilder<QueryableMod1> _feature;

    public SubqueryBuilderTest()
    {
        _feature = new SubqueryBuilder<QueryableMod1>();
    }

    [Fact]
    public void Should_create_avg_subquery_from_property()
    {
        _feature.Avg(a => a.PropertyID);

        var actual = _feature.GetSqlStatement();

        Assert.Equal("(SELECT AVG(PropertyID) AS [AvgResult] FROM Table1)", actual);
    }

    [Fact]
    public void Should_create_subquery_with_where_condition()
    {
        _feature.Avg(a => a.PropertyID);
        _feature.Where(a => a.PropertyID == 99);

        var actual = _feature.GetSqlStatement();

        Assert.Equal("(SELECT AVG(PropertyID) AS [AvgResult] FROM Table1 WHERE PropertyID = @sub10)", actual);
    }

    [Fact]
    public void Should_create_select_function_from_sql_function_methods()
    {
        _feature.Select(a => SqlFunc.Sum(SqlFunc.IIF(a.PropertyID >= 12, 1, 0)));

        var actual = _feature.GetSqlStatement();

        Assert.Equal("(SELECT SUM(IIF(PropertyID >= 12, 1, 0)) FROM Table1)", actual);
    }

    [Fact]
    public void Should_create_select_function_from_sql_function_and_operators()
    {
        _feature.Select(a => SqlFunc.Sum(SqlFunc.IIF(a.PropertyID >= 12, 1, 0)) / 100);

        var actual = _feature.GetSqlStatement();

        Assert.Equal("(SELECT SUM(IIF(PropertyID >= 12, 1, 0)) / @sub10 FROM Table1)", actual);
        Assert.Single(_feature.Parameters);
    }
}