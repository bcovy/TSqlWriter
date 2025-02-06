using SqlWriter.Integration.Fixtures;

namespace SqlWriter.Integration.Builders.Query;

public class QueryBuilderTTest
{
    private readonly IQuery<QueryableMod1> _feature;

    public QueryBuilderTTest()
    {
        _feature = SqlWriters.Query<QueryableMod1>("a");
    }

    #region Select
    [Fact]
    public void Select_one_column()
    {
        _feature.Select(a => new { a.PropertyID });

        var actual = _feature.GetSqlStatement();

        Assert.StartsWith("SELECT a.PropertyID FROM Table1 AS a", actual);
    }

    [Fact]
    public void Select_subquery_column()
    {
        string expected = "SELECT (SELECT PropertyID FROM Table2 WHERE PropertyID = @sub10) AS [sub1]";

        _feature.SelectSubquery("sub1", () =>
        {
            return SqlWriters.Subquery<QueryableMod2>()
                .Select(a => a.PropertyID)
                .Where(a => a.PropertyID == 11);
        });
        var actual = _feature.GetSqlStatement();

        Assert.StartsWith(expected, actual);
        Assert.Single(_feature.GetParameters);
    }

    #endregion Select

    #region With CTE
    [Fact]
    public void With_should_add_cte_statement()
    {
        ICteStatement cte = SqlWriters.QueryAsCte<QueryableMod1>("cteA")
            .Select(a => new { a.PropertyID, a.Address }).Where(a => a.Address == "hello world");

        _feature.Select(a => new { a.PropertyID });
        _feature.With(a => a.PropertyID, cte);
        var actual = _feature.GetSqlStatement();

        Assert.StartsWith("WITH cteA AS (SELECT a.PropertyID", actual);
        Assert.Contains("a.PropertyID, cteA.Address", actual);
        Assert.Contains("a.PropertyID = cteA.PropertyID", actual);
    }

    [Fact]
    public void With_should_add_cte_statement_using_join_type_expression()
    {
        ICteStatement cte = SqlWriters.QueryAsCte<QueryableMod1>("cteA")
            .Select(a => new { a.PropertyID, a.Address }).Where(a => a.Address == "hello world");

        _feature.Select(a => new { a.PropertyID });
        _feature.With<QueryableMod1>((a, b) => a.PropertyID == b.PropertyID, cte);
        var actual = _feature.GetSqlStatement();

        Assert.StartsWith("WITH cteA AS (SELECT a.PropertyID", actual);
        Assert.Contains("a.PropertyID, cteA.Address", actual);
        Assert.Contains("a.PropertyID = cteA.PropertyID", actual);
    }

    [Fact]
    public void With_should_add_cte_statement_and_not_project_columns_into_parent_query()
    {
        var cte = SqlWriters.QueryAsCte<QueryableMod5>("PropertyID", stopColumnsProjection: true).Select(a => new { a.Address, a.PropertyID });

        _feature.Select(a => new { a.PropertyID });
        _feature.With(a => a.PropertyID, cte);
        var actual = _feature.GetSqlStatement();

        Assert.Contains("SELECT a.PropertyID FROM Table1 AS a", actual);
    }

    #endregion With CTE

    #region  Where
    [Fact]
    public void Select_with_one_where_expression()
    {
        _feature.Select(a => new { a.PropertyID });
        _feature.Where(a => a.PropertyID == 99);
        var actual = _feature.GetSqlStatement();

        Assert.Contains("WHERE a.PropertyID = @p0", actual);
    }

    [Fact]
    public void Select_exists_method()
    {
        _feature.Select(a => new { a.PropertyID });
        _feature.WhereExists<QueryableMod2>((a, b) => a.PropertyID == b.PropertyID & a.Address == "hello");
        var actual = _feature.GetSqlStatement();

        Assert.Equal("SELECT a.PropertyID FROM Table1 AS a\n WHERE EXISTS (SELECT * FROM Table2 AS ext WHERE ext.PropertyID = a.PropertyID AND ext.Address = @p0)", actual);
    }

    [Fact]
    public void Select_exists_method_with_outer_query_condition()
    {
        _feature.Select(a => new { a.PropertyID });
        _feature.WhereExists<QueryableMod2>((a, b) => a.PropertyID == b.PropertyID & a.Address == "hello");
        _feature.Where(a => a.FirstName == "duder");
        var actual = _feature.GetSqlStatement();

        Assert.Equal("SELECT a.PropertyID FROM Table1 AS a\n WHERE EXISTS (SELECT * FROM Table2 AS ext WHERE ext.PropertyID = a.PropertyID AND ext.Address = @p0) AND a.FirstName = @p1", actual);
    }

    #endregion Where

    #region Group by
    [Fact]
    public void Group_by_one_column()
    {
        _feature.GroupBy(a => a.FirstName);
        var actual = _feature.GetSqlStatement();

        Assert.Contains("GROUP BY a.FirstName", actual);
    }

    [Fact]
    public void Group_by_two_columns()
    {
        _feature.GroupBy(a => new { a.FirstName, a.LastName });
        var actual = _feature.GetSqlStatement();

        Assert.EndsWith("GROUP BY a.FirstName, a.LastName", actual);
    }

    #endregion Group by

    #region Order by
    [Fact]
    public void Order_by_one_column()
    {
        _feature.OrderByAsc(a => a.FirstName);
        var actual = _feature.GetSqlStatement();

        Assert.EndsWith("ORDER BY a.FirstName ASC", actual);
    }

    [Fact]
    public void Order_by_two_columns()
    {
        _feature.OrderByAsc(a => a.FirstName);
        _feature.OrderByDesc(b => b.LastName);
        var actual = _feature.GetSqlStatement();

        Assert.EndsWith("ORDER BY a.FirstName ASC, a.LastName DESC", actual);
    }

    #endregion Order by
}