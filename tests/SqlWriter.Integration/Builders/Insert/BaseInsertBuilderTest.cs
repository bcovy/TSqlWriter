using System.Linq.Expressions;
using SqlWriter.Builders.Insert;
using SqlWriter.Components.Tables;
using SqlWriter.Integration.Fixtures;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Integration.Builders.Insert;

public class TestFeature(ITablesManager tables, Type insertEntity, string concatStatement = "")
    : BaseInsertBuilder(tables, ManagersFixture.GetParameterManager(), insertEntity, "p", concatStatement)
{
}

public class BaseInsertBuilderTest
{
    private readonly TablesManager _tables;
    private readonly TestFeature _feature;

    public BaseInsertBuilderTest()
    {
        _tables = new(typeof(QueryableMod3), "a");
        _feature = new(_tables, typeof(QueryableMod3));
    }

    [Fact]
    public void InsertColumnsFromProjection_creates_insert_column_list_from_class()
    {
        _feature.InsertColumnsFromProjection<QueryableMod1>();

        Assert.Equal("PropertyID", _feature.InsertTargets[0]);
        Assert.Equal("Address", _feature.InsertTargets[1]);
        Assert.Equal("FirstName", _feature.InsertTargets[2]);
    }

    [Fact]
    public void InsertColumnsFromExpression_should_add_columns_in_order_of_expression_input()
    {
        Expression<Func<QueryableMod1, object>> expression = a => new { a.Address, a.PropertyID, a.PcoeDate };

        _feature.InsertColumnsFromExpression(expression);

        Assert.Equal("Address", _feature.InsertTargets[0]);
        Assert.Equal("PropertyID", _feature.InsertTargets[1]);
        Assert.Equal("PcoeDate", _feature.InsertTargets[2]);
    }

    [Fact]
    public void BuildStatement_using_default_into_projection()
    {
        _feature.SelectBuilder.AddProjection<QueryableMod3>();

        string result = _feature.BuildStatement();

        Assert.Equal(6, _feature.InsertTargets.Count);
        Assert.StartsWith("INSERT INTO Table3 (PropertyID, Address, PcoeDate, TaskStatus, DecimalVal, DecimalValNull)", result);
    }

    [Fact]
    public void CompileSql_with_output_clause_creates_valid_sql_statement()
    {
        string expected = "INSERT INTO Table3 (Address, PropertyID)\n OUTPUT Inserted.Address, Inserted.PropertyID INTO Table1 (Address, PropertyID)\n SELECT a.Address, a.PropertyID FROM Table3 AS a\n";
        TestFeature sut = new(_tables, typeof(QueryableMod3));

        Expression<Func<QueryableMod3, object>> expression = a => new { a.Address, a.PropertyID };
        sut.InsertColumnsFromExpression(expression);
        sut.SelectBuilder.TranslateExpression(expression, "s");
        sut.OutputTo<QueryableMod3, QueryableMod1>(a => new QueryableMod1() { Address = a.Address, PropertyID = a.PropertyID });
        var actual = sut.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompileSql_with_concat_statement_creates_valid_sql_statement()
    {
        string expected = "SELECT * FROM SomeTable;\nINSERT INTO Table3 (Address, PropertyID)\n SELECT a.Address, a.PropertyID FROM Table3 AS a\n";
        TestFeature sut = new(_tables, typeof(QueryableMod3), "SELECT * FROM SomeTable");

        Expression<Func<QueryableMod3, object>> expression = a => new { a.Address, a.PropertyID };
        sut.InsertColumnsFromExpression(expression);
        sut.SelectBuilder.TranslateExpression(expression, "s");
        var actual = sut.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ConcatWithRowCount_creates_valid_sql_statement()
    {
        string expected = "INSERT INTO Table3 (Address, PropertyID)\n SELECT a.Address, a.PropertyID FROM Table3 AS a\n;\nIF @@ROWCOUNT = 0\nBEGIN     RETURN\nEND ";

        Expression<Func<QueryableMod3, object>> expression = a => new { a.Address, a.PropertyID };
        _feature.InsertColumnsFromExpression(expression);
        _feature.SelectBuilder.TranslateExpression(expression, "s");
        var actual = _feature.ConcatWithRowCount();

        Assert.Equal(expected, actual.SqlStatement);
    }
}
