using SqlWriter.Builders.Update;
using SqlWriter.Components.Parameters;
using SqlWriter.Components.Tables;
using SqlWriter.Integration.Fixtures;
using SqlWriter.Integration.Mocks;

namespace SqlWriter.Integration.Builders.Update;

public class UpdateBuilderTTest
{
    private readonly UpdateBuilderT<QueryableMod3, QueryableMod1> _feature;

    public UpdateBuilderTTest()
    {
        TablesManager tables1 = new(typeof(QueryableMod3), "a");
        tables1.AddTable<QueryableMod1>("b");
        tables1.AddJoin<QueryableMod3, QueryableMod1>(JoinType.Inner);
        _feature = new(tables1, new ParameterManager());
    }

    [Fact]
    public void Set_using_argument_value()
    {
        _feature.Set(a => a.DecimalVal, 101.25M);

        Assert.Equal("DecimalVal = @p0", _feature.Columns[0]);
        Assert.Contains(101.25M, _feature.Parameters.Select(x => x.Value));
    }

    [Fact]
    public void Set_using_field_value()
    {
        int value = 101;

        _feature.Set(a => a.PropertyID, value);

        Assert.Equal("PropertyID = @p0", _feature.Columns[0]);
        Assert.Contains(101, _feature.Parameters.Select(x => x.Value));
    }

    [Fact]
    public void Set_using_expression_with_property_field_int_value()
    {
        var model = new QueryableMod3() { PropertyID = 99 };

        _feature.Set(a => a.PropertyID, model.PropertyID);

        Assert.Equal("PropertyID = @p0", _feature.Columns[0]);
        Assert.Contains(99, _feature.Parameters.Select(x => x.Value));
    }

    [Fact]
    public void Set_using_dateonly_nullable_struct_type()
    {
        _feature.Set(a => a.PcoeDate, DateOnly.FromDateTime(DateTime.Now));

        Assert.Equal("PcoeDate = @p0", _feature.Columns[0]);
    }

    [Fact]
    public void Set_value_using_empty_string_field_property_should_produce_null_parameter_value()
    {
        Command = new();
        _feature.Set(a => a.Address, Command.Address);

        var actual = _feature.Columns.FirstOrDefault();

        Assert.Equal("Address = @p0", actual);
    }

    public QueryableMod1 Command { get; set; }

    [Fact]
    public void Set_using_sql_function_method_call_type()
    {
        _feature.Set(a => a.PropertyID, b => SqlFunc.Average(b.PropertyID));

        Assert.Equal("PropertyID = AVG(b.PropertyID)", _feature.Columns[0]);
    }

    [Fact]
    public void Set_expression_using_join_entity()
    {
        _feature.Set(a => a.TaskStatus, b => b.PropertyID);

        Assert.Equal("TaskStatus = b.PropertyID", _feature.Columns[0]);
    }
    
    [Fact]
    public void SetNull_value_on_target_column()
    {
        _feature.SetNull(a => a.PcoeDate);

        Assert.Equal("PcoeDate = NULL", _feature.Columns[0]);
    }

    [Fact]
    public void OutputTo_inserted_values()
    {
#if OSX
        string expected = $"UPDATE Table3 SET PropertyID = @p0\nOUTPUT Inserted.Address INTO Table2 (Address)\n FROM Table3 AS a\n JOIN Table1 AS b ON a.PropertyID = b.PropertyID";
#else
        string expected = $"UPDATE Table3 SET PropertyID = @p0\nOUTPUT Inserted.Address INTO Table2 (Address)\r\n FROM Table3 AS a\n JOIN Table1 AS b ON a.PropertyID = b.PropertyID";
#endif
        _feature.Set(a => a.PropertyID, 101);
        _feature.OutputTo(o => new UpdateOutput<QueryableMod2>() 
        { 
            Inserted = new QueryableMod2 { Address = o.Address } 
        });

        string actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Where_subquery_condition_for_expression_target_column()
    {
        _feature.WhereSubquery(a => a.PropertyID, SubqueryMock.SubqueryFunc());

        var actual = _feature.GetSqlStatement();

        Assert.EndsWith(" WHERE a.PropertyID = (SELECT PropertyID FROM Table2)", actual);
    }

    [Fact]
    public void Compile_sql_with_where_exists()
    {
#if OSX
        string expected = "UPDATE Table3 SET PropertyID = @p0\n FROM Table3 AS a\n JOIN Table1 AS b ON a.PropertyID = b.PropertyID WHERE EXISTS (SELECT * FROM Table2 AS ext WHERE ext.PropertyID = b.PropertyID)";
#else
        string expected = "UPDATE Table3 SET PropertyID = @p0\r\n FROM Table3 AS a\n JOIN Table1 AS b ON a.PropertyID = b.PropertyID WHERE EXISTS (SELECT * FROM Table2 AS ext WHERE ext.PropertyID = b.PropertyID)";
#endif
        _feature.Set(a => a.PropertyID, 101);
        _feature.WhereExists<QueryableMod2>((a, b) => a.PropertyID == b.PropertyID);

        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
        Assert.Single(_feature.Parameters);
    }

    [Fact]
    public void Compile_sql_with_two_set_statements_and_additional_join_table_source()
    {
#if OSX
        string expected = "UPDATE Table3 SET PropertyID = @p0, Address = @p1\n FROM Table3 AS a\n JOIN Table1 AS b ON a.PropertyID = b.PropertyID";
#else
        string expected = "UPDATE Table3 SET PropertyID = @p0, Address = @p1\r\n FROM Table3 AS a\n JOIN Table1 AS b ON a.PropertyID = b.PropertyID";
#endif
        _feature.Set(a => a.PropertyID, 101);
        _feature.Set(a => a.Address, "hello world");

        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
        Assert.Equal(2, _feature.Parameters.Count());
    }

    [Fact]
    public void Compile_sql_with_fluent_concat_update_extension_method()
    {
#if OSX
        string expected = "UPDATE Table3 SET PropertyID = @p0\n FROM Table3 AS a\n JOIN Table1 AS b ON a.PropertyID = b.PropertyID;\nUPDATE Table3 SET Address = @p1\n FROM Table3 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID";
#else
        string expected = "UPDATE Table3 SET PropertyID = @p0\r\n FROM Table3 AS a\n JOIN Table1 AS b ON a.PropertyID = b.PropertyID;\nUPDATE Table3 SET Address = @p1\r\n FROM Table3 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID";
#endif
        var sut = SqlWriters.Update<QueryableMod3, QueryableMod1>()
            .Set(a => a.PropertyID, 9)
            .Concat().Update<QueryableMod3, QueryableMod2>().Set(a => a.Address, "hello world");
        var actual = sut.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Compile_sql_with_broken_concat_update_extension_method()
    {
#if OSX
        string expected = "UPDATE Table3 SET PropertyID = @p0\n FROM Table3 AS a\n JOIN Table1 AS b ON a.PropertyID = b.PropertyID;\nUPDATE Table3 SET Address = @p1\n FROM Table3 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID";
#else
        string expected = "UPDATE Table3 SET PropertyID = @p0\r\n FROM Table3 AS a\n JOIN Table1 AS b ON a.PropertyID = b.PropertyID;\nUPDATE Table3 SET Address = @p1\r\n FROM Table3 AS a\n JOIN Table2 AS b ON a.PropertyID = b.PropertyID";
#endif

        var sut = SqlWriters.Update<QueryableMod3, QueryableMod1>().Set(a => a.PropertyID, 9).Concat();
        var actual = sut.Update<QueryableMod3, QueryableMod2>().Set(a => a.Address, "hello world").GetSqlStatement();

        Assert.Equal(expected, actual);
    }
}
