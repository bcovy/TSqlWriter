using System.Linq.Expressions;
using SqlWriter.Components.Parameters;
using SqlWriter.Components.Select;
using SqlWriter.Integration.Fixtures;
using SqlWriter.Interfaces.Internals;
using SqlWriter.Translators;

namespace SqlWriter.Integration.Components;

public class SelectBuilderTest
{
    private readonly ITablesManager _tables;
    private readonly IExpressionSqlTranslator _translator;
    private readonly ParameterManager _parameterManager;
    private readonly SelectBuilder _feature;

    public SelectBuilderTest()
    {
        _tables = ManagersFixture.GetTablesManager(false);
        _parameterManager = ManagersFixture.GetParameterManager();
        _translator = new ExpressionSqlTranslator(_tables, _parameterManager);
        _feature = new SelectBuilder(_translator, _tables, _parameterManager);
    }

    [Fact]
    public void Translate_expression_to_statement()
    {
        Expression<Func<QueryableMod1, object>> expression = a => new { a.Address, a.PropertyID, a.PcoeDate };

        _feature.TranslateExpression(expression);
        string actual = _feature.Compile();

        Assert.Equal("SELECT a.Address, a.PropertyID, a.PcoeDate", actual);
    }

    [Fact]
    public void Translate_expression_with_function_that_has_no_associated_column()
    {
        Expression<Func<QueryableMod1, object>> expression = a => new { RowCount = SqlFunc.Count() };

        _feature.TranslateExpression(expression);
        string actual = _feature.Compile();

        Assert.Equal("SELECT COUNT(*) AS [RowCount]", actual);
    }

    [Fact]
    public void Translate_expression_with_nested_functions_that_require_further_recursion_by_visitor()
    {
        Expression<Func<QueryableMod1, object>> expression = a => new { RowCount = SqlFunc.Cast(SqlFunc.Round(SqlFunc.Average(a.PropertyID * 1.0M), 0), "INT") };

        _feature.TranslateExpression(expression);
        string actual = _feature.Compile();

        Assert.Equal("SELECT CAST(ROUND(AVG(a.PropertyID * 1.0), 0) AS INT) AS [RowCount]", actual);
    }

    [Fact]
    public void Translate_expression_with_function_alias_name_that_matches_entity_column()
    {
        Expression<Func<QueryableMod1, object>> expression = a => new { PropertyID = SqlFunc.IIF(a.PropertyID == 1, 2, 3) };

        _feature.TranslateExpression(expression);
        string actual = _feature.Compile();

        Assert.Equal("SELECT IIF(a.PropertyID = 1, 2, 3) AS [PropertyID]", actual);
    }

    [Fact]
    public void Select_column_should_parameterize_constant_type_value_reference()
    {
        Expression<Func<QueryableMod1, object>> expression = a => new { a.Address, PropID = 1 };

        _feature.TranslateExpression(expression);
        string actual = _feature.Compile();

        Assert.Equal("SELECT a.Address, @sel0 AS [PropID]", actual);
        Assert.Single(_parameterManager.Parameters);
    }

    [Fact]
    public void Select_column_should_parameterize_value_derived_from_field_property()
    {
        Projection2 model = new() { PropertyID = 99 };
        Expression<Func<QueryableMod1, object>> expression = a => new { a.Address, PropID = model.PropertyID };

        _feature.TranslateExpression(expression);
        string actual = _feature.Compile();

        Assert.Equal("SELECT a.Address, @sel0 AS [PropID]", actual);
        var parameter = Assert.Single(_parameterManager.Parameters);
        Assert.Equal(99, parameter.Value);
    }

    [Fact]
    public void Select_column_targets_that_includes_parameter()
    {
        Expression<Func<QueryableMod1, object>> expression = a => new { a.Address, PropID = a.PropertyID + 1 };

        _feature.TranslateExpression(expression);
        string actual = _feature.Compile();

        Assert.Equal("SELECT a.Address, a.PropertyID + @sel0 AS [PropID]", actual);
    }

    [Fact]
    public void Compile_statement_twice_with_additional_column()
    {
        Expression<Func<QueryableMod1, object>> expression = a => new { a.Address, PropID = 1 };

        _feature.TranslateExpression(expression);
        string actual1 = _feature.Compile();
        _feature.AddColumn("SomeColumn", "SomeColumn");
        string actual2 = _feature.Compile();

        Assert.Equal("SELECT a.Address, @sel0 AS [PropID]", actual1);
        Assert.Equal("SELECT a.Address, @sel0 AS [PropID], SomeColumn", actual2);
    }

    #region Select projection

    [Fact]
    public void Select_using_activator_projection_entity_from_two_tables()
    {
        _tables.AddTable<QueryableMod2>("b");

        _feature.AddProjection<Projection2>();
        string actual = _feature.Compile();

        Assert.Equal("SELECT a.PropertyID, b.DescID, b.EventID, a.Address", actual);
    }

    public class Projection2
    {
        public int PropertyID { get; set; }
        public short? DescID { get; set; }
        public int EventID { get; set; }
        public string Address { get; set; }
        public int OverallCnt { get; set; }
    }

    #endregion Select projection
}