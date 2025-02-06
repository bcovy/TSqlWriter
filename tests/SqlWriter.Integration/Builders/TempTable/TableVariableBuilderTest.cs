using SqlWriter.Builders.TempTable;
using SqlWriter.Integration.Fixtures;

namespace SqlWriter.Integration.Builders.TempTable;

public class TableVariableBuilderTest
{
    [Fact]
    public void Init_sets_string_column_with_size_attribute()
    {
        var actual = new TableVariableBuilder(typeof(TempTable4), ManagersFixture.GetParameterManager());

        Assert.Contains("TaskNumber VARCHAR (10)", actual.Fields);
    }

    [Fact]
    public void Init_sets_decimal_column_with_precision_attribute()
    {
        var actual = new TableVariableBuilder(typeof(TempTable3), ManagersFixture.GetParameterManager());

        Assert.Contains("TaskNumber DECIMAL (2, 1)", actual.Fields);
    }

    [Fact]
    public void Init_sets_string_column_with_sql_db_type_attribute()
    {
        var actual = new TableVariableBuilder(typeof(TempTable2), ManagersFixture.GetParameterManager());

        Assert.Contains("TaskNumber SmallInt", actual.Fields);
    }
    
    [Fact]
    public void CompiledSql_creates_temp_table_from_entity_that_has_identified_typename()
    {
        string expected = "DECLARE @TableTmp TABLE (EventID Int, UserName VARCHAR(1000))";
        var feature = SqlWriters.TableVariable<TempTable1>();

        var actual = feature.GetSqlStatement();
        string statement = actual;

        Assert.Equal(expected, statement);
    }

    [Fact]
    public void CompiledSql_creates_temp_table_from_entity_that_has_identified_sqldbtype()
    {
        string expected = "DECLARE @TableTmp2 TABLE (EventID Int, TaskNumber SmallInt)";
        var feature = SqlWriters.TableVariable<TempTable2>();

        var actual = feature.GetSqlStatement();
        string statement = actual;

        Assert.Equal(expected, statement);
    }

    [Fact]
    public void Concat_creates_valid_sql_block()
    {
        string expected = "DECLARE @TableTmp2 TABLE (EventID Int, TaskNumber SmallInt);\nDECLARE @TableTmp TABLE (EventID Int, UserName VARCHAR(1000))";
        var feature = SqlWriters.TableVariable<TempTable2>().Concat().TableVariable<TempTable1>();

        var actual = feature.Concat();
        string statement = actual.SqlStatement;

        Assert.Equal(expected, statement);
    }
}
