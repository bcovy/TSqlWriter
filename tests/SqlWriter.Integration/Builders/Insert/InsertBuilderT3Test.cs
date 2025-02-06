using SqlWriter.Integration.Fixtures;

namespace SqlWriter.Integration.Builders.Insert;

public class InsertBuilderT3Test
{
    private readonly IInsert<QueryableMod4, QueryableMod3, QueryableMod2, QueryableMod1> _feature;

    public InsertBuilderT3Test()
    {
        _feature = SqlWriters.Insert<QueryableMod4, QueryableMod3, QueryableMod2, QueryableMod1>().Join<QueryableMod3, QueryableMod2>()
            .Join<QueryableMod2, QueryableMod1>();
    }

    [Fact]
    public void CompileStatement_should_produce_qualified_statement_when_init_is_made_with_entity_pk()
    {
        string expected = "INSERT INTO Table4 (Address, PropertyID, PcoeDate)\n SELECT @p0 AS [Address], a.PropertyID, b.PcoeDate FROM Table3 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID JOIN Table1 AS c ON b.PropertyID = c.PropertyID";

        _feature.Into(a => new { a.Address, a.PropertyID, a.PcoeDate });
        _feature.Select((a, b, c) => new { Address = "hello world", a.PropertyID, b.PcoeDate });
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
        Assert.Single(_feature.Parameters);
    }

    [Fact]
    public void CompileStatement_should_match_select_projection_columns_with_insert_targets()
    {
        string expected = "INSERT INTO Table4 (Address, PropertyID, PcoeDate)\n SELECT a.Address, a.PropertyID, c.PcoeDate";

        _feature.Into<Projection>();
        _feature.Select((a, b, c) => new { a.Address, a.PropertyID, c.PcoeDate });
        var actual = _feature.GetSqlStatement();

        Assert.StartsWith(expected, actual);
    }

    [Fact]
    public void CompileStatement_with_where_condition()
    {
        string expected = "INSERT INTO Table4 (Address, PropertyID, PcoeDate)\n SELECT a.Address, a.PropertyID, a.PcoeDate FROM Table3 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID JOIN Table1 AS c ON b.PropertyID = c.PropertyID WHERE a.Address = @p0";

        _feature.Into<Projection>();
        _feature.Select((a, b, c) => new { a.Address, a.PropertyID, a.PcoeDate });
        _feature.Where((a, b, c) => a.Address == "hello world");
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompileStatement_with_group_by()
    {
        string expected = "INSERT INTO Table4 (Address, PropertyID, PcoeDate)\n SELECT a.Address, a.PropertyID, a.PcoeDate FROM Table3 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID JOIN Table1 AS c ON b.PropertyID = c.PropertyID GROUP BY a.Address";

        _feature.Into<Projection>();
        _feature.Select((a, b, c) => new { a.Address, a.PropertyID, a.PcoeDate });
        _feature.GroupBy((a, b, c) => a.Address);
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompileStatement_with_having()
    {
        string expected = "INSERT INTO Table4 (Address)\n SELECT a.Address FROM Table3 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID JOIN Table1 AS c ON b.PropertyID = c.PropertyID GROUP BY a.Address HAVING COUNT(*) > 1";

        _feature.Into(a => new { a.Address });
        _feature.Select((a, b, c) => new { a.Address });
        _feature.GroupBy((a, b, c) => a.Address);
        _feature.Having((a, b, c) => SqlFunc.Count() > 1);
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompileStatement_with_output_to()
    {
        _feature.Into(a => new { a.Address });
        _feature.OutputTo(a => new QueryableMod2 { Address = a.Address });
        _feature.Select((a, b, c) => new { a.Address });
        var actual = _feature.GetSqlStatement();

        Assert.Contains("OUTPUT Inserted.Address INTO Table2 (Address)", actual);
    }

    [Fact]
    public void CompileStatement_with_concat_insert_extension_method()
    {
        string expected = "INSERT INTO Table2 (Address) VALUES (@p0);\nINSERT INTO Table3 (Address)\n SELECT a.Address FROM Table4 AS a\n JOIN Table1 AS b ON a.PropertyID = b.PropertyID";
        var sut = SqlWriters.Insert<QueryableMod2>()
            .Set(a => a.Address, "hello")
            .Concat()
            .Insert<QueryableMod3, QueryableMod4, QueryableMod1>()
            .Into(a => new { a.Address })
            .Select((a, b) => new { a.Address })
            .Join<QueryableMod4, QueryableMod1>();
        var actual = sut.GetSqlStatement();

        Assert.Equal(expected, actual);
    }
}
