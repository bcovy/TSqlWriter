using SqlWriter.Builders.Insert;
using SqlWriter.Components.Parameters;
using SqlWriter.Components.Tables;
using SqlWriter.Integration.Fixtures;

namespace SqlWriter.Integration.Builders.Insert;

public class InsertBuilderTest
{
    private readonly TablesManager _tables;
    private readonly InsertBuilder<QueryableMod3> _feature;

    public InsertBuilderTest()
    {
        _tables = new(typeof(QueryableMod3), "a");
        _feature = new InsertBuilder<QueryableMod3>(_tables, ManagersFixture.GetParameterManager());
    }

    #region Set
    [Fact]
    public void Set_using_numeric_int_value_should_produce_parameterized_statement()
    {
        _feature.Set(a => a.PropertyID, 101);

        var actual = _feature.Columns.FirstOrDefault();

        Assert.Equal("PropertyID", actual.Item1);
        Assert.Equal("@p0", actual.Item2);
    }

    [Fact]
    public void Set_using_expression_with_non_numeric_property_field()
    {
        var model = new QueryableMod3() { Address = "hello" };
        _feature.Set(a => a.Address, model.Address);

        var actual = _feature.Columns.FirstOrDefault();

        Assert.Equal("Address", actual.Item1);
        Assert.Equal("@p0", actual.Item2);
    }

    [Fact]
    public void Set_value_with_subquery()
    {
        string expected = "(SELECT PropertyID FROM Table2 WHERE PropertyID = @sub10)";

        _feature.SetSubquery(a => a.PropertyID, () =>
        {
            return SqlWriters.Subquery<QueryableMod2>()
                .Select(a => a.PropertyID)
                .Where(a => a.PropertyID == 11);
        });
        var actual = _feature.Columns.FirstOrDefault();

        Assert.Equal("PropertyID", actual.Item1);
        Assert.Equal(expected, actual.Item2);
    }

    [Fact]
    public void Set_using_expression_with_raw_value()
    {
        _feature.SetRaw(a => a.Address, "hello world");

        var actual = _feature.Columns.FirstOrDefault();

        Assert.Equal("Address", actual.Item1);
        Assert.Equal("hello world", actual.Item2);
    }

    [Fact]
    public void Set_using_raw_values()
    {
        _feature.SetRaw("Address", "hello world");

        var actual = _feature.Columns.FirstOrDefault();

        Assert.Equal("Address", actual.Item1);
        Assert.Equal("hello world", actual.Item2);
    }

    #endregion Set

    #region Compile
    [Fact]
    public void CompiledSql_returns_full_sql_insert_statement()
    {
        string expected = $"INSERT INTO Table3 (Address, PropertyID) VALUES (@p0, @p1)";

        _feature.Set(a => a.Address, "hello");
        _feature.Set(a => a.PropertyID, 9);

        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompiledSql_with_subquery_returns_full_sql_insert_statement()
    {
        string expected = "INSERT INTO Table3 (Address, PropertyID) VALUES (@p0, (SELECT PropertyID FROM Table2 WHERE PropertyID = @sub10))";

        _feature.Set(a => a.Address, "hello world");
        _feature.SetSubquery(a => a.PropertyID, () =>
        {
            return SqlWriters.Subquery<QueryableMod2>()
                .Select(a => a.PropertyID)
                .Where(a => a.PropertyID == 11);
        });
        var actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
        Assert.Equal(2, _feature.GetParameters.Count);
    }

    [Fact]
    public void CompileSql_with_concat_statement_creates_valid_sql_statement()
    {
        string expected = "SELECT * FROM SomeTable;\nINSERT INTO Table3 (Address) VALUES (@p0)";
        var sut = new InsertBuilder<QueryableMod3>(_tables, new ParameterManager(), concatSqlStatement: "SELECT * FROM SomeTable");

        sut.Set(a => a.Address, "hello world");
        var actual = sut.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ConcatWithRowCount_returns_statement_with_if_rowcount_block()
    {
        string expected = $"INSERT INTO Table3 (Address, PropertyID) VALUES (@p0, @p1);\nIF @@ROWCOUNT = 0\nBEGIN     RETURN\nEND ";

        _feature.Set(a => a.Address, "hello");
        _feature.Set(a => a.PropertyID, 9);

        var actual = _feature.ConcatWithRowCount();

        Assert.Equal(expected, actual.SqlStatement);
    }

    #endregion Compile
}