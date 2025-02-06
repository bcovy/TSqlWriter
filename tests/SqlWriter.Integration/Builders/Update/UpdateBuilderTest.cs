using SqlWriter.Builders.Update;
using SqlWriter.Components.Parameters;
using SqlWriter.Integration.Fixtures;
using SqlWriter.Integration.Mocks;

namespace SqlWriter.Integration.Builders.Update;

public class UpdateBuilderTest
{
    private readonly UpdateBuilder<QueryableMod3> _feature;

    public UpdateBuilderTest()
    {
        _feature = new(new ParameterManager());
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
    public void Set_using_date_only_nullable_struct_type()
    {
        _feature.Set(a => a.PcoeDate, DateOnly.FromDateTime(DateTime.Now));

        Assert.Equal("PcoeDate = @p0", _feature.Columns[0]);
    }

    [Fact]
    public void Set_value_using_empty_string_field_property_should_produce_null_parameter_value()
    {
        Command = new();

        _feature.Set(a => a.Address, Command.Address);

        Assert.Equal("Address = @p0", _feature.Columns[0]);
    }

    public QueryableMod1 Command { get; set; }

    [Fact]
    public void Set_using_sql_function_method_call_type()
    {
        _feature.Set(a => a.PropertyID, b => SqlFunc.Average(b.PropertyID));

        Assert.Equal("PropertyID = AVG(PropertyID)", _feature.Columns[0]);
    }
    
    [Fact]
    public void SetNull_value_on_target_column()
    {
        _feature.SetNull(a => a.PcoeDate);

        Assert.Equal("PcoeDate = NULL", _feature.Columns[0]);
    }

    [Fact]
    public void Set_subquery_condition_for_target_column()
    {
        _feature.SetSubquery(a => a.PropertyID, SubqueryMock.Subquery());

        Assert.Equal("PropertyID = (SELECT PropertyID FROM Table2)", _feature.Columns[0]);
    }

    [Fact]
    public void Where_subquery_condition_for_expression_target_column()
    {
        _feature.WhereSubquery(a => a.PropertyID, SubqueryMock.SubqueryFunc());
        var actual = _feature.GetSqlStatement();

        Assert.EndsWith(" WHERE PropertyID = (SELECT PropertyID FROM Table2)", actual);
    }

    [Fact]
    public void Compile_sql_with_two_set_statements()
    {
        _feature.Set(a => a.PropertyID, 101);
        _feature.Set(a => a.Address, "hello world");
        var actual = _feature.GetSqlStatement();

        Assert.Equal("UPDATE Table3 SET PropertyID = @p0, Address = @p1", actual);
        Assert.Equal(2, _feature.Parameters.Count());
    }

    [Fact]
    public void Compile_sql_with_fluent_concat_update_extension_method()
    {
        string expected = "UPDATE Table2 SET PropertyID = @p0;\nUPDATE Table3 SET Address = @";

        var sut = SqlWriters.Update<QueryableMod2>().Set(a => a.PropertyID, 9)
            .Concat().Update<QueryableMod3>().Set(a => a.Address, "hello world");
        var actual = sut.GetSqlStatement();

        Assert.StartsWith(expected, actual);
    }

    [Fact]
    public void Compile_sql_with_broken_concat_update_extension_method()
    {
        string expected = "UPDATE Table2 SET PropertyID = @p0;\nUPDATE Table3 SET Address = @";

        var sut = SqlWriters.Update<QueryableMod2>().Set(a => a.PropertyID, 9).Concat();
        var actual = sut.Update<QueryableMod3>().Set(a => a.Address, "hello world").GetSqlStatement();

        Assert.StartsWith(expected, actual);
    }
}
