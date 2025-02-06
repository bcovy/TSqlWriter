using SqlWriter.Integration.Fixtures;

namespace SqlWriter.Integration.Builders.Insert;

public class InsertBuilderT4Test
{
    private readonly IInsert<QueryableMod4, QueryableMod3, QueryableMod2, QueryableMod1, QueryableMod5> _feature;

    public InsertBuilderT4Test()
    {
        _feature = SqlWriters.Insert<QueryableMod4, QueryableMod3, QueryableMod2, QueryableMod1, QueryableMod5>()
            .Join<QueryableMod3, QueryableMod2>()
            .Join<QueryableMod2, QueryableMod1>()
            .Join<QueryableMod2, QueryableMod5>(JoinType.Left);
    }

    [Fact]
    public void CompileStatement_should_produce_qualified_statement_when_init_is_made_with_entity_pk()
    {
        string expected = "INSERT INTO Table4 (Address, PropertyID, PcoeDate)\n SELECT @p0 AS [Address], c.PropertyID, d.PcoeDate FROM Table3 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID JOIN Table1 AS c ON b.PropertyID = c.PropertyID LEFT OUTER JOIN Table5 AS d ON b.PropertyID = d.PropertyID";

        _feature.Into(a => new { a.Address, a.PropertyID, a.PcoeDate });
        _feature.Select((a, b, c, d) => new { Address = "hello world", c.PropertyID, d.PcoeDate });
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
        Assert.Single(_feature.Parameters);
    }

    [Fact]
    public void CompileStatement_with_where_condition()
    {
        string expected = "INSERT INTO Table4 (Address)\n SELECT a.Address FROM Table3 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID JOIN Table1 AS c ON b.PropertyID = c.PropertyID LEFT OUTER JOIN Table5 AS d ON b.PropertyID = d.PropertyID WHERE a.Address = @p0";

        _feature.Into(a => new { a.Address });
        _feature.Select((a, b, c, d) => new { a.Address});
        _feature.Where((a, b, c, d) => a.Address == "hello world");
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompileStatement_with_group_by()
    {
        string expected = "INSERT INTO Table4 (Address)\n SELECT a.Address FROM Table3 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID JOIN Table1 AS c ON b.PropertyID = c.PropertyID LEFT OUTER JOIN Table5 AS d ON b.PropertyID = d.PropertyID GROUP BY a.Address";

        _feature.Into(a => new { a.Address });
        _feature.Select((a, b, c, d) => new { a.Address });
        _feature.GroupBy((a, b, c, d) => a.Address);
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompileStatement_with_having()
    {
        string expected = "INSERT INTO Table4 (Address)\n SELECT a.Address FROM Table3 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID JOIN Table1 AS c ON b.PropertyID = c.PropertyID LEFT OUTER JOIN Table5 AS d ON b.PropertyID = d.PropertyID GROUP BY a.Address HAVING COUNT(*) > 1";

        _feature.Into(a => new { a.Address });
        _feature.Select((a, b, c, d) => new { a.Address });
        _feature.GroupBy((a, b, c, d) => a.Address);
        _feature.Having((a, b, c, d) => SqlFunc.Count() > 1);
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompileStatement_with_output_to()
    {
        _feature.Into(a => new { a.Address });
        _feature.OutputTo(a => new QueryableMod2 { Address = a.Address });
        _feature.Select((a, b, c, d) => new { a.Address });
        var actual = _feature.GetSqlStatement();

        Assert.Contains("OUTPUT Inserted.Address INTO Table2 (Address)", actual);
    }
}
