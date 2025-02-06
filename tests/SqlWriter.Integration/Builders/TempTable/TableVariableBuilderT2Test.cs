using SqlWriter.Integration.Fixtures;

namespace SqlWriter.Integration.Builders.TempTable;

public class TableVariableBuilderT2Test
{
    [Fact]
    public void Concat_creates_valid_sql_block()
    {
        string expected = "DECLARE @TableTmp2 TABLE (EventID Int, TaskNumber SmallInt);\nDECLARE @TableTmp TABLE (EventID Int, UserName VARCHAR(1000))";
        var feature = SqlWriters.TableVariables<TempTable2, TempTable1>();

        var actual = feature.Concat();
        string statement = actual.SqlStatement;

        Assert.Equal(expected, statement);
    }
}
