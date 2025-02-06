using SqlWriter.Infrastructure;
using SqlWriter.Tests.Fixtures;

namespace SqlWriter.Tests.Infrastructure;

public class TableNameHelperTest
{
    [Fact]
    public void GetName_from_table_name_attribute()
    {
        string actual = TableNameHelper.GetName(typeof(QueryableMod2));

        Assert.Equal("Table2", actual);
    }

    [Fact]
    public void GetTableName_from_table_variable_attribute()
    {
        string actual = TableNameHelper.GetName(typeof(TempTable));

        Assert.Equal("@TableTmp", actual);
    }

    [Fact]
    public void GetName_throws_error_if_table_naming_attributes_are_missing()
    {
        Assert.Throws<MissingFieldException>(() => TableNameHelper.GetName(typeof(QueryableMod1)));
    }
}
