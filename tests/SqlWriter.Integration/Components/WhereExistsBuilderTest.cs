using SqlWriter.Components.Where;
using SqlWriter.Integration.Fixtures;
using SqlWriter.Interfaces.Internals;
using SqlWriter.Translators;

namespace SqlWriter.Integration.Components;

public class WhereExistsBuilderTest
{
    private readonly ITablesManager _tables;
    private readonly IExpressionSqlTranslator _translator;
    private readonly WhereExistsBuilder<QueryableMod2> _feature;

    public WhereExistsBuilderTest()
    {
        _tables = ManagersFixture.GetTablesManager();
        var parameterManager1 = ManagersFixture.GetParameterManager();
        _translator = new ExpressionSqlTranslator(_tables, parameterManager1);
        _feature = new(_translator, _tables);
    }

    [Fact]
    public void Compile_with_where_join_condition_only()
    {
        string expected = "EXISTS (SELECT * FROM Table2 AS ext WHERE ext.PropertyID = a.PropertyID)";

        string actual = _feature.Compile<QueryableMod1>((a, b) => a.PropertyID == b.PropertyID);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Compile_as_not_exist_statement()
    {
        string expected = "NOT EXISTS (SELECT * FROM Table2 AS ext WHERE ext.PropertyID = a.PropertyID)";
        WhereExistsBuilder<QueryableMod2> sut = new(_translator, _tables, true);

        string actual = sut.Compile<QueryableMod1>((a, b) => a.PropertyID == b.PropertyID);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Compile_with_where_join_condition_and_additional_statement()
    {
        string expected = "EXISTS (SELECT * FROM Table2 AS ext WHERE ext.PropertyID = a.PropertyID AND ext.EventID = @pw0)";

        string actual = _feature.Compile<QueryableMod1>((a, b) => a.PropertyID == b.PropertyID & a.EventID == 99);

        Assert.Equal(expected, actual);
    }
}
