using System.Data;
using SqlWriter.Integration.Fixtures;

namespace SqlWriter.Integration.Builders.RawSql;

public class RawSqlBuilderTest
{
    [Fact]
    public void Adds_sql_statement_at_instantiation()
    {
        var feature = SqlWriters.RawSql("hello world");
        string actual = feature.GetSqlStatement();
        
        Assert.Equal("hello world", actual);
    }
    
    [Fact]
    public void Add_sql_statement_can_handle_interpolated_field_value()
    {
        string world = "world";
        
        var feature = SqlWriters.RawSql($"hello {world}");
        string actual = feature.GetSqlStatement();
        
        Assert.Equal("hello world", actual);
    }
    
    [Fact]
    public void Add_sql_statement_can_handle_interpolated_member_value()
    {
        QueryableMod1 world = new() { Address = "world" };
        
        var feature = SqlWriters.RawSql($"hello {world.Address}");
        string actual = feature.GetSqlStatement();
        
        Assert.Equal("hello world", actual);
    }
    
    [Fact]
    public void AddParameter_should_preserves_parameter_name()
    {
        var feature = SqlWriters.RawSql("hello world").AddParameter(99, "hello");
        
        var target = Assert.Single(feature.Parameters);
        Assert.Equal("@hello", target.ParameterName);
        Assert.Equal(SqlDbType.Int, target.SqlDataType);
    }
    
    [Fact]
    public void AddParameter_should_save_user_supplied_sql_db_type()
    {
        var feature = SqlWriters.RawSql("hello world").AddParameter(99, "hello", SqlDbType.Bit);
        
        var target = Assert.Single(feature.Parameters);
        Assert.Equal("@hello", target.ParameterName);
        Assert.Equal(SqlDbType.Bit, target.SqlDataType);
    }
    
    [Fact]
    public void Compile_sql_with_fluent_concat_update_extension_method()
    {
        string expected = "UPDATE Table2 SET PropertyID = @p0;\nUPDATE Table3 SET Address = @Ad1";

        var sut = SqlWriters.Update<QueryableMod2>().Set(a => a.PropertyID, 9)
            .Concat().RawSql("UPDATE Table3 SET Address = @Ad1");
        var actual = sut.GetSqlStatement();

        Assert.Equal(expected, actual);
    }
}