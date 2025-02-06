using SqlWriter.Integration.Fixtures;

namespace SqlWriter.Integration.Builders.Query;

public class QueryBuilderT2Test
{
    private readonly IQuery<QueryableMod1, QueryableMod2> _feature;

    public QueryBuilderT2Test()
    {
        _feature = SqlWriters.Query<QueryableMod1, QueryableMod2>();
    }

    #region CTE Join
    [Fact]
    public void With_should_add_cte_statement_using_join_type_expression()
    {
#if OSX
        string expected = "WITH cteA AS (SELECT a.PropertyID, a.Address FROM Table1 AS a\n)\nSELECT a.PropertyID, cteA.Address FROM Table1 AS a\n JOIN cteA ON a.PropertyID = cteA.PropertyID";
#else
        string expected = "WITH cteA AS (SELECT a.PropertyID, a.Address FROM Table1 AS a\n)\r\nSELECT a.PropertyID, cteA.Address FROM Table1 AS a\n JOIN cteA ON a.PropertyID = cteA.PropertyID";
#endif
        ICteStatement cte = SqlWriters.QueryAsCte<QueryableMod1>("cteA").Select(a => new { a.PropertyID, a.Address });

        _feature.Select((a, b) => new { a.PropertyID });
        _feature.With<QueryableMod1>((a, b, c) => a.PropertyID == c.PropertyID, cte);
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void With_should_add_cte_statement_using_composite_join_type_expression()
    {
#if OSX
        string expected = "WITH cteA AS (SELECT a.PropertyID, a.Address, a.FirstName FROM Table1 AS a\n)\nSELECT a.PropertyID, cteA.FirstName FROM Table1 AS a\n JOIN cteA ON a.PropertyID = cteA.PropertyID AND a.Address = cteA.Address";
#else
        string expected = "WITH cteA AS (SELECT a.PropertyID, a.Address, a.FirstName FROM Table1 AS a\n)\r\nSELECT a.PropertyID, cteA.FirstName FROM Table1 AS a\n JOIN cteA ON a.PropertyID = cteA.PropertyID AND a.Address = cteA.Address";
#endif
        ICteStatement cte = SqlWriters.QueryAsCte<QueryableMod1>("cteA").Select(a => new { a.PropertyID, a.Address, a.FirstName });

        _feature.Select((a, b) => new { a.PropertyID });
        _feature.With<QueryableMod1>((a, b, c) => a.PropertyID == c.PropertyID & a.Address == c.Address, cte);
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    #endregion CTE Join

    #region Where
    [Fact]
    public void Select_exists_method()
    {
        string expected = "SELECT a.PropertyID FROM Table1 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID WHERE EXISTS (SELECT * FROM Table3 AS ext WHERE ext.PropertyID = b.PropertyID AND ext.Address = @p0)";
        
        _feature.Select((a, b) => new { a.PropertyID });
        _feature.Join<QueryableMod1, QueryableMod2>();
        _feature.WhereExists<QueryableMod3, QueryableMod2>((a, b) => a.PropertyID == b.PropertyID & a.Address == "hello");
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Select_exists_method_with_outer_query_condition()
    {
        string expected = "SELECT a.PropertyID FROM Table1 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID WHERE EXISTS (SELECT * FROM Table3 AS ext WHERE ext.PropertyID = a.PropertyID AND ext.Address = @p1) AND b.Address = @p0";
        
        _feature.Select((a, b) => new { a.PropertyID });
        _feature.Join<QueryableMod1, QueryableMod2>();
        _feature.Where((a, b) => b.Address == "duder");
        _feature.WhereExists<QueryableMod3, QueryableMod1>((a, b) => a.PropertyID == b.PropertyID & a.Address == "hello");
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    #endregion Where
}
