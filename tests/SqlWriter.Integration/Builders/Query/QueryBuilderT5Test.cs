using SqlWriter.Integration.Fixtures;

namespace SqlWriter.Integration.Builders.Query;

public class QueryBuilderT5Test
{
    
    [Fact]
    public void With_should_add_cte_statement_using_join_type_expression()
    {
#if OSX
        string expected = "WITH cteA AS (SELECT a.PropertyID, a.Address FROM Table1 AS a\n)\nSELECT a.PropertyID, cteA.PropertyID, cteA.Address FROM Table1 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID LEFT OUTER JOIN Table3 AS c ON b.PropertyID = c.PropertyID JOIN Table4 AS d ON b.PropertyID = d.PropertyID LEFT OUTER JOIN Table5 AS e ON a.PropertyID = e.PropertyID JOIN cteA ON a.Address = cteA.Address WHERE b.EventID = @p0";
#else
        string expected = "WITH cteA AS (SELECT a.PropertyID, a.Address FROM Table1 AS a\n)\nSELECT a.PropertyID, cteA.PropertyID, cteA.Address FROM Table1 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID LEFT OUTER JOIN Table3 AS c ON b.PropertyID = c.PropertyID JOIN Table4 AS d ON b.PropertyID = d.PropertyID LEFT OUTER JOIN Table5 AS e ON a.PropertyID = e.PropertyID JOIN cteA ON a.Address = cteA.Address WHERE b.EventID = @p0";
#endif
        ICteStatement cte = SqlWriters.QueryAsCte<QueryableMod1>("cteA", includeCteJoinColumn: true).Select(a => new { a.PropertyID, a.Address });

        var statement = SqlWriters.Query<QueryableMod1, QueryableMod2, QueryableMod3, QueryableMod4, QueryableMod5>()
            .Select((a, b, c, d, e) => new { a.PropertyID })
            .Join(join =>
            {
                join.Inner<QueryableMod1, QueryableMod2>();
                join.Left<QueryableMod2, QueryableMod3>((a, b) => a.PropertyID == b.PropertyID);
                join.Inner<QueryableMod2, QueryableMod4>();
                join.Left<QueryableMod1, QueryableMod5>((a, b) => a.PropertyID == b.PropertyID);
            })
            .With<QueryableMod1>((a, b, c, d, e, f) => a.Address == b.Address, cte)
            .Where((a, b, c, d, e) => b.EventID == 1);

        var actual = statement.GetSqlStatement();

        Assert.Equal(expected, actual);
    }
}
