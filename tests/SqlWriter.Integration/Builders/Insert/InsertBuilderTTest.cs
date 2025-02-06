using SqlWriter.Integration.Fixtures;

namespace SqlWriter.Integration.Builders.Insert;

public class InsertBuilderTTest
{
    private readonly IInsert<QueryableMod2, QueryableMod1> _feature;

    public InsertBuilderTTest()
    {
        _feature = SqlWriters.Insert<QueryableMod2, QueryableMod1>();
    }

    [Fact]
    public void CompileStatement_should_match_select_columns_with_insert_targets()
    {
        string expected = "INSERT INTO Table2 (Address, PropertyID, PcoeDate)\n SELECT a.Address, a.PropertyID, a.PcoeDate FROM Table1 AS a\n";

        _feature.Into(a => new { a.Address, a.PropertyID, a.PcoeDate });
        _feature.Select(a => new { a.Address, a.PropertyID, a.PcoeDate });
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompileStatement_should_match_select_projection_columns_with_insert_targets()
    {
        string expected = "INSERT INTO Table2 (Address, PropertyID, PcoeDate)\n SELECT a.Address, a.PropertyID, a.PcoeDate FROM Table1 AS a\n";

        _feature.Into<Projection>();
        _feature.Select<Projection>();
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompileStatement_should_produce_qualified_statement_with_parameters()
    {
        string expected = "INSERT INTO Table2 (Address, PropertyID, PcoeDate)\n SELECT @p0 AS [Address], a.PropertyID, a.PcoeDate FROM Table1 AS a\n";

        _feature.Into(a => new { a.Address, a.PropertyID, a.PcoeDate });
        _feature.Select(a => new { Address = "hello world", a.PropertyID, a.PcoeDate });
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
        Assert.Single(_feature.Parameters);
    }

    [Fact]
    public void CompileStatement_with_where_condition()
    {
        string expected = "INSERT INTO Table2 (Address, PropertyID, PcoeDate)\n SELECT a.Address, a.PropertyID, a.PcoeDate FROM Table1 AS a\n WHERE a.Address = @p0";

        _feature.Into<Projection>();
        _feature.Select(a => new { a.Address, a.PropertyID, a.PcoeDate });
        _feature.Where(a => a.Address == "hello world");
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompileStatement_with_group_by()
    {
        string expected = "INSERT INTO Table2 (Address, PropertyID, PcoeDate)\n SELECT a.Address, a.PropertyID, a.PcoeDate FROM Table1 AS a\n GROUP BY a.Address";

        _feature.Into<Projection>();
        _feature.Select(a => new { a.Address, a.PropertyID, a.PcoeDate });
        _feature.GroupBy(a => a.Address);
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompileStatement_with_having()
    {
        string expected = "INSERT INTO Table2 (Address)\n SELECT a.Address FROM Table1 AS a\n GROUP BY a.Address HAVING COUNT(*) > 1";

        _feature.Into(a => new { a.Address });
        _feature.Select(a => new { a.Address });
        _feature.GroupBy(a => a.Address);
        _feature.Having(a => SqlFunc.Count() > 1);
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompileStatement_with_output_to()
    {
        string expected = "INSERT INTO Table2 (Address)\n OUTPUT Inserted.Address INTO Table2 (Address)\n SELECT a.Address FROM Table1 AS a\n";

        _feature.Into(a => new { a.Address });
        _feature.OutputTo(a => new QueryableMod2 { Address = a.Address });
        _feature.Select(a => new { a.Address });
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompileStatement_with_concat_insert_extension_method()
    {
        string expected = "INSERT INTO Table2 (Address)\n SELECT a.Address FROM Table1 AS a\n;\nINSERT INTO Table3 (Address)\n SELECT a.Address FROM Table4 AS a\n";
        var sut = SqlWriters.Insert<QueryableMod2, QueryableMod1>()
            .Into(a => new { a.Address })
            .Select(a => new { a.Address })
            .Concat().Insert<QueryableMod3, QueryableMod4>().Into(a => new { a.Address }).Select(a => new { a.Address });
        var actual = sut.GetSqlStatement();

        Assert.Equal(expected, actual);
    }
}