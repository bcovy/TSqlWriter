using System.Linq.Expressions;
using SqlWriter.Components.Where;
using SqlWriter.Integration.Fixtures;
using SqlWriter.Integration.Mocks;
using SqlWriter.Interfaces.Internals;
using SqlWriter.Translators;

namespace SqlWriter.Integration.Components;

public class WhereBuilderTest
{
    private readonly ITablesManager _tables;
    private readonly WhereBuilder _feature;

    public WhereBuilderTest()
    {
        _tables = ManagersFixture.GetTablesManager();
        var parameterManager1 = ManagersFixture.GetParameterManager();
        IExpressionSqlTranslator translator1 = new ExpressionSqlTranslator(_tables, parameterManager1);
        _feature = new WhereBuilder(translator1, parameterManager1);
    }

    #region Add column and value
    [Fact]
    public void Add_column_and_value_as_first_condition_should_exclude_andor_prefix()
    {
        _feature.AddColumnAndValue("PropertyID", 99, Predicates.Equal);
        string actual = _feature.Compile();

        Assert.Equal(" WHERE PropertyID = 99", actual);
    }

    [Fact]
    public void Add_column_and_value_as_second_condition_should_include_andor_prefix()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.PropertyID == 99;

        _feature.TranslateExpression(expression);
        _feature.AddColumnAndValue("a.LastName", "@pw1", Predicates.Equal);
        string actual = _feature.Compile();

        Assert.Equal(" WHERE a.PropertyID = @pw0 AND a.LastName = @pw1", actual);
    }

    [Fact]
    public void WhereOr_should_set_OR_prefix_when_used_with_two_seperate_column_and_value_calls()
    {
        _feature.AddColumnAndValue("a.LastName", "@p0", Predicates.Equal);
        _feature.WhereOr();
        _feature.AddColumnAndValue("a.FirstName", "@p1", Predicates.Equal);
        _feature.AddColumnAndValue("a.ThirdName", "@p2", Predicates.Equal);
        string actual = _feature.Compile();

        Assert.Equal(" WHERE a.LastName = @p0 OR a.FirstName = @p1 AND a.ThirdName = @p2", actual);
    }

    [Fact]
    public void Add_column_and_value_as_like_condition()
    {
        _feature.AddColumnAndValue("PropertyID", 99, Predicates.Like);
        string actual = _feature.Compile();

        Assert.Equal(" WHERE PropertyID LIKE 99", actual);
    }

    [Fact]
    public void Add_column_and_value_as_between_condition()
    {
        _feature.AddColumnAndValue("PropertyID", "0 AND 100", Predicates.Between);
        string actual = _feature.Compile();

        Assert.Equal(" WHERE PropertyID BETWEEN 0 AND 100", actual);
    }

    #endregion Add column and value

    #region Exists
    [Fact]
    public void AddExists_is_inserted_as_first_condition()
    {
        _feature.AddColumnAndValue("PropertyID", "0 AND 100", Predicates.Between);
        _feature.AddExist("exist statement");
        string actual = _feature.Conditions[0].Item2;

        Assert.Equal("exist statement", actual);
    }

    #endregion Exists

    #region Translate
    [Fact]
    public void Translate_where_condition_expression()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.PropertyID == 99 & a.LastName == "name";

        _feature.TranslateExpression(expression);
        string actual = _feature.Compile();

        Assert.Equal(" WHERE a.PropertyID = @pw0 AND a.LastName = @pw1", actual);
    }

    [Fact]
    public void Translate_with_two_seperate_expression_condition_calls_should_include_and_prefix()
    {
        Expression<Func<QueryableMod1, bool>> expression1 = (a) => a.PropertyID == 99;
        Expression<Func<QueryableMod1, bool>> expression2 = (a) => a.LastName == "name";

        _feature.TranslateExpression(expression1);
        string actual1 = _feature.Compile();
        _feature.TranslateExpression(expression2);
        string actual2 = _feature.Compile();

        Assert.Equal(" WHERE a.PropertyID = @pw0", actual1);
        Assert.Equal(" WHERE a.PropertyID = @pw0 AND a.LastName = @pw1", actual2);
    }

    [Fact]
    public void Translate_with_two_seperate_expression_condition_calls_should_include_or_prefix()
    {
        Expression<Func<QueryableMod1, bool>> expression1 = (a) => a.PropertyID == 99;
        Expression<Func<QueryableMod1, bool>> expression2 = (a) => a.LastName == "name";

        _feature.TranslateExpression(expression1);
        string actual1 = _feature.Compile();
        _feature.WhereOr();
        _feature.TranslateExpression(expression2);
        string actual2 = _feature.Compile();

        Assert.Equal(" WHERE a.PropertyID = @pw0", actual1);
        Assert.Equal(" WHERE a.PropertyID = @pw0 OR a.LastName = @pw1", actual2);
    }

    #endregion Translate

    #region Subquery
    [Fact]
    public void Add_subquery_condition_for_target_column()
    {
        ISubquery subquery = SubqueryMock.Subquery();
        Expression<Func<QueryableMod1, int>> expression = (a) => a.PropertyID;

        _feature.WhereSubquery(expression, subquery);
        string actual = _feature.Compile();

        Assert.Equal(" WHERE a.PropertyID = (SELECT PropertyID FROM Table2)", actual);
    }

    [Fact]
    public void Add_subquery_condition_for_target_column_excluding_alias()
    {
        ISubquery subquery = SubqueryMock.Subquery();
        Expression<Func<QueryableMod1, int>> expression = (a) => a.PropertyID;

        _feature.WhereSubquery(expression, subquery, true);
        string actual = _feature.Compile();

        Assert.Equal(" WHERE PropertyID = (SELECT PropertyID FROM Table2)", actual);
    }

    [Fact]
    public void Add_subquery_with_additional_parent_column_where_conditon()
    {
        ISubquery subquery = SubqueryMock.Subquery();
        Expression<Func<QueryableMod1, int>> expression = (a) => a.PropertyID;

        _feature.AddColumnAndValue("Address", "hello", Predicates.Equal);
        _feature.WhereSubquery(expression, subquery, true);
        string actual = _feature.Compile();

        Assert.Equal(" WHERE Address = hello AND PropertyID = (SELECT PropertyID FROM Table2)", actual);
    }

    #endregion Subquery

    #region Compile
    [Fact]
    public void Compile_using_column_value_then_translation_should_produce_two_conditions_with_and_prefix()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.PropertyID == 99;

        _feature.AddColumnAndValue("PropertyID", 99, Predicates.Like);
        _feature.TranslateExpression(expression);
        string actual = _feature.Compile();

        Assert.Equal(" WHERE PropertyID LIKE 99 AND a.PropertyID = @pw0", actual);
    }

    [Fact]
    public void Compile_statement_twice_with_additional_condition()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.PropertyID == 99 & a.LastName == "name";

        _feature.TranslateExpression(expression);
        string actual1 = _feature.Compile();
        _feature.AddColumnAndValue("PropertyID", 99, Predicates.Like);
        string actual2 = _feature.Compile();

        Assert.Equal(" WHERE a.PropertyID = @pw0 AND a.LastName = @pw1", actual1);
        Assert.Equal(" WHERE a.PropertyID = @pw0 AND a.LastName = @pw1 AND PropertyID LIKE 99", actual2);
    }

    #endregion Compile
}