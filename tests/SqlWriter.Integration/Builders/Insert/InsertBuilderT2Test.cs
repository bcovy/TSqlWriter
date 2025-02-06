using SqlWriter.Integration.Fixtures;

namespace SqlWriter.Integration.Builders.Insert;

public class InsertBuilderT2Test
{
    private readonly IInsert<QueryableMod4, QueryableMod3, QueryableMod2> _feature;

    public InsertBuilderT2Test()
    {
        _feature = SqlWriters.Insert<QueryableMod4, QueryableMod3, QueryableMod2>().Join<QueryableMod3, QueryableMod2>();
    }

    [Fact]
    public void CompileStatement_should_match_select_projection_columns_with_insert_targets()
    {
        string expected = "INSERT INTO Table4 (Address, PropertyID, EventID)\n SELECT a.Address, a.PropertyID, b.EventID";

        _feature.Into<Projection2>();
        _feature.Select<Projection2>();
        var actual = _feature.GetSqlStatement();

        Assert.StartsWith(expected, actual);
    }

    [Fact]
    public void CompileStatement_should_match_select_columns_with_insert_targets()
    {
        string expected = "INSERT INTO Table4 (Address, PropertyID, PcoeDate)\n SELECT a.Address, a.PropertyID, b.PcoeDate";

        _feature.Into(a => new { a.Address, a.PropertyID, a.PcoeDate });
        _feature.Select((a, b) => new { a.Address, a.PropertyID, b.PcoeDate });
        var actual = _feature.GetSqlStatement();

        Assert.StartsWith(expected, actual);
    }

    [Fact]
    public void CompileStatement_with_no_into_method()
    {
        string expected = "INSERT INTO Table4 (PropertyID, Table4ID, Address, PcoeDate, TaskStatus)\n SELECT a.PropertyID, b.EventID AS [ID], a.Address, b.PcoeDate, a.TaskStatus";

        _feature.Select((a, b) => new { a.PropertyID, ID = b.EventID, a.Address, b.PcoeDate, a.TaskStatus });
        var actual = _feature.GetSqlStatement();

        Assert.StartsWith(expected, actual);
    }

    [Fact]
    public void CompileStatement_should_produce_qualified_statement_with_parameters()
    {
        string expected = "INSERT INTO Table4 (Address, PropertyID, PcoeDate)\n SELECT @p0 AS [Address], a.PropertyID, b.PcoeDate FROM Table3 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID";

        _feature.Into(a => new { a.Address, a.PropertyID, a.PcoeDate });
        _feature.Select((a, b) => new { Address = "hello world", a.PropertyID, b.PcoeDate });
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
        Assert.Single(_feature.Parameters);
    }

    [Fact]
    public void CompileStatement_compiles_when_insert_and_select_table_types_are_the_same()
    {
        string expected = "INSERT INTO Table4 (Address, PropertyID, PcoeDate)\n SELECT a.Address, a.PropertyID, b.PcoeDate";
        IInsert<QueryableMod4, QueryableMod3, QueryableMod4> sut = SqlWriters.Insert<QueryableMod4, QueryableMod3, QueryableMod4>()
            .Join((a, b) => a.PropertyID == b.PropertyID);

        sut.Into(a => new { a.Address, a.PropertyID, a.PcoeDate });
        sut.Select((a, b) => new { a.Address, a.PropertyID, b.PcoeDate });
        var actual = sut.GetSqlStatement();

        Assert.StartsWith(expected, actual);
    }

    [Fact]
    public void CompileStatement_with_where_condition()
    {
        string expected = "INSERT INTO Table4 (Address, PropertyID, PcoeDate)\n SELECT a.Address, a.PropertyID, a.PcoeDate FROM Table3 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID WHERE a.Address = @p0";

        _feature.Into<Projection>();
        _feature.Select((a, b) => new { a.Address, a.PropertyID, a.PcoeDate });
        _feature.Where((a, b) => a.Address == "hello world");
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompileStatement_with_group_by()
    {
        string expected = "INSERT INTO Table4 (Address, PropertyID, PcoeDate)\n SELECT a.Address, a.PropertyID, a.PcoeDate FROM Table3 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID GROUP BY a.Address";

        _feature.Into<Projection>();
        _feature.Select((a, b) => new { a.Address, a.PropertyID, a.PcoeDate });
        _feature.GroupBy((a, b) => a.Address);
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompileStatement_with_having()
    {
        string expected = "INSERT INTO Table4 (Address)\n SELECT a.Address FROM Table3 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID GROUP BY a.Address HAVING COUNT(*) > 1";

        _feature.Into(a => new { a.Address });
        _feature.Select((a, b) => new { a.Address });
        _feature.GroupBy((a, b) => a.Address);
        _feature.Having((a, b) => SqlFunc.Count() > 1);
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompileStatement_with_output_to()
    {
        string expected = "INSERT INTO Table4 (Address)\n OUTPUT Inserted.Address INTO Table2 (Address)\n SELECT a.Address FROM Table3 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID";

        _feature.Into(a => new { a.Address });
        _feature.OutputTo(a => new QueryableMod2 { Address = a.Address });
        _feature.Select((a, b) => new { a.Address });
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompileStatement_with_concat_insert_extension_method()
    {
        string expected = "INSERT INTO Table2 (Address)\n SELECT a.Address FROM Table1 AS a\n;\nINSERT INTO Table3 (Address)\n SELECT a.Address FROM Table4 AS a\n JOIN Table1 AS b ON a.PropertyID = b.PropertyID";
        var sut = SqlWriters.Insert<QueryableMod2, QueryableMod1>()
            .Into(a => new { a.Address })
            .Select(a => new { a.Address })
            .Concat()
            .Insert<QueryableMod3, QueryableMod4, QueryableMod1>()
            .Into(a => new { a.Address })
            .Select((a, b) => new { a.Address })
            .Join<QueryableMod4, QueryableMod1>();
        var actual = sut.GetSqlStatement();

        Assert.Equal(expected, actual);
    }
}