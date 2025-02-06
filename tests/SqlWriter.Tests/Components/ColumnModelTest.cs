using System.Data;
using SqlWriter.Components.Tables;
using SqlWriter.Tests.Fixtures;

namespace SqlWriter.Tests.Components;

public class ColumnModelTest
{
    [Fact]
    public void Init_sets_int_column_with_no_defined_attributes()
    {
        var actual = new ColumnModel("PropertyID", typeof(QueryableMod6), "a");

        Assert.Equal(SqlDbType.Int, actual.SqlDataType);
        Assert.Null(actual.TypeName);
    }

    [Fact]
    public void Init_sets_string_column_with_no_defined_attributes()
    {
        var actual = new ColumnModel("EmptyString", typeof(QueryableMod6), "a");

        Assert.Equal(SqlDbType.VarChar, actual.SqlDataType);
        Assert.Null(actual.TypeName);
    }

    [Fact]
    public void Init_sets_decimal_column_with_no_defined_attributes()
    {
        var actual = new ColumnModel("EmptyDec", typeof(QueryableMod6), "a");

        Assert.Equal(SqlDbType.Decimal, actual.SqlDataType);
        Assert.Null(actual.TypeName);
    }

    [Fact]
    public void Init_sets_string_column_with_sql_type_attribute()
    {
        var actual = new ColumnModel("Address", typeof(QueryableMod6), "a");

        Assert.Equal(SqlDbType.VarChar, actual.SqlDataType);
        Assert.Equal("VarChar (101)", actual.TypeName);
    }

    [Fact]
    public void Init_sets_string_column_with_size_attribute()
    {
        var actual = new ColumnModel("FirstName", typeof(QueryableMod6), "a");

        Assert.Equal(SqlDbType.VarChar, actual.SqlDataType);
        Assert.Equal("VARCHAR (125)", actual.TypeName);
    }

    [Fact]
    public void Init_sets_decimal_column_with_precision_attribute()
    {
        var actual = new ColumnModel("Money", typeof(QueryableMod6), "a");

        Assert.Equal(SqlDbType.Decimal, actual.SqlDataType);
        Assert.Equal("DECIMAL (3, 3)", actual.TypeName);
    }

    [Fact]
    public void Init_sets_nullable_decimal_column_with_precision_attribute()
    {
        var actual = new ColumnModel("MoneyNullable", typeof(QueryableMod6), "a");

        Assert.Equal(SqlDbType.Decimal, actual.SqlDataType);
        Assert.Equal("DECIMAL (4, 2)", actual.TypeName);
    }

    [Fact]
    public void Init_set_string_column_with_sql_type_and_overrides_additional_attributes()
    {
        var actual = new ColumnModel("StringOverride", typeof(QueryableMod6), "a");

        Assert.Equal(SqlDbType.NVarChar, actual.SqlDataType);
        Assert.Equal("VarChar (101)", actual.TypeName);
    }
}
