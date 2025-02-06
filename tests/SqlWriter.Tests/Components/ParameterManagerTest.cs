using SqlWriter.Components.Parameters;
using SqlWriter.Components.Tables;
using SqlWriter.Tests.Fixtures;

namespace SqlWriter.Tests.Components;

public class ParameterManagerTest
{
    private readonly ParameterManager _feature;

    public ParameterManagerTest()
    {
        _feature = new ParameterManager();
    }

    [Fact]
    public void Add_parameter_generic_type_should_add_parameter_to_list_and_return_parameter_name()
    {
        string actual = _feature.Add(99);
        var expected = _feature.Parameters.FirstOrDefault();

        Assert.NotNull(expected);
        Assert.Equal("@p0", actual);
        Assert.Equal("p0", expected.ParameterNameRaw);
        Assert.Equal("p0", expected.GetSqlDataParameter.ParameterName);
    }

    [Fact]
    public void Add_parameter_applies_input_parameter_name()
    {
        string actual = _feature.Add(99, "somep");
        var expected = _feature.Parameters.FirstOrDefault();

        Assert.NotNull(expected);
        Assert.Equal("@somep0", actual);
        Assert.Equal("somep0", expected.ParameterNameRaw);
        Assert.Equal("somep0", expected.GetSqlDataParameter.ParameterName);
    }

    [Fact]
    public void Add_parameter_generic_type_with_column_model_should_add_parameter_to_list_and_return_parameter_name()
    {
        ColumnModel column = new("PropertyID", typeof(QueryableMod1), "a");

        string actual = _feature.Add(column, 99, "hello");
        var expected = _feature.Parameters.FirstOrDefault();

        Assert.NotNull(expected);
        Assert.Equal("@hello0", actual);
        Assert.Equal("hello0", expected.GetSqlDataParameter.ParameterName);
    }

    [Fact]
    public void Add_multiple_parameters_should_increment_parameter_name_count()
    {
        ColumnModel column = new("PropertyID", typeof(QueryableMod1), "a");

        string actual1 = _feature.Add(column, 99);
        string actual2 = _feature.Add(99);

        Assert.Equal("@p0", actual1);
        Assert.Equal("@p1", actual2);
        Assert.Equal(2, _feature.Parameters.Count);
    }
    
    [Fact]
    public void GetParameters_returns_key_value_pairs_with_no_at_character()
    {
        string actual = _feature.Add(99, "somep");
        var expected = _feature.GetParameters.FirstOrDefault();
        
        Assert.Equal("somep0", expected.Key);
        Assert.Equal(99, expected.Value);
    }
}