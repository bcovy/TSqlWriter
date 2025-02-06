using SqlWriter.Builders.Truncate;
using SqlWriter.Integration.Fixtures;

namespace SqlWriter.Integration.Builders.Truncate;

public class TruncateBuilderTest
{
    [Fact]
    public void BuildStatement_returns_valid_sql_statement()
    {
        var feature = new TruncateBuilder(typeof(QueryableMod2), ManagersFixture.GetParameterManager());
        var actual = feature.GetSqlStatement();

        Assert.Equal("TRUNCATE TABLE Table2", actual);
    }

    [Fact]
    public void BuildStatement_returns_valid_statement_with_prefix_concat_sql()
    {
        var feature = new TruncateBuilder(typeof(QueryableMod2), ManagersFixture.GetParameterManager(), concatSqlStatement: "some sql");
        var actual = feature.Concat();

        Assert.Equal("some sql;\nTRUNCATE TABLE Table2", actual.SqlStatement);
    }
}
