using System.Linq.Expressions;
using SqlWriter.Components.Parameters;
using SqlWriter.Integration.Fixtures;
using SqlWriter.Translators;

namespace SqlWriter.Integration.Translators;

public class ExpressionSqlTranslatorTest
{
    private readonly ExpressionSqlTranslator _feature;
    private readonly ParameterManager _parameterManager;

    public ExpressionSqlTranslatorTest()
    {
        _parameterManager = ManagersFixture.GetParameterManager();
        _feature = new ExpressionSqlTranslator(ManagersFixture.GetTablesManager(true), _parameterManager);
    }

    public CommandModel Command { get; set; }

    public class CommandModel
    {
        public int PropertyID { get; set; }
        public string Name { get; set; }
        public int? TaskNumber { get; set; }
    }

    #region Properties
    [Fact]
    public void Should_translate_nullable_type_property()
    {
        Expression<Func<QueryableMod1, DateTime?>> expression = (a) => a.PcoeDate;

        string actual = _feature.Translate(expression);

        Assert.Equal("a.PcoeDate", actual);
    }

    [Fact]
    public void Should_translate_property_expression_for_node_type_of_convert()
    {
        Expression<Func<QueryableMod1, int?>> expression = (a) => a.PropertyID;

        string actual = _feature.Translate(expression);

        Assert.Equal("a.PropertyID", actual);
    }

    #endregion Properties

    #region Single constraint statements
    [Fact]
    public void Single_constraint_with_constant_value()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.PropertyID == 99;

        string actual = _feature.Translate(expression);

        Assert.Equal("a.PropertyID = @p0", actual);
    }

    [Fact]
    public void Single_constraint_with_column_that_does_not_include_table_alias_reference()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.PropertyID == 99;

        string actual = _feature.TranslateWithoutAlias(expression);

        Assert.Equal("PropertyID = @p0", actual);
    }

    [Fact]
    public void Single_constraint_with_constant_value_un_parameterized()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.PropertyID == 99;

        string actual = _feature.Translate(expression, doNotParameterizeValues: true);

        Assert.Equal("a.PropertyID = 99", actual);
    }

    [Fact]
    public void Single_constraint_with_constant_variable_value()
    {
        int value = 99;
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.PropertyID == value;

        string actual = _feature.Translate(expression);

        Assert.Equal("a.PropertyID = @p0", actual);
        Assert.Contains(99, _parameterManager.Parameters.Select(x => x.Value));
    }

    [Fact]
    public void Single_constraint_with_constant_nullable_variable_value()
    {
        int? value = 99;
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.PropertyID == value;

        string actual = _feature.Translate(expression);

        Assert.Equal("a.PropertyID = @p0", actual);
        Assert.Contains(99, _parameterManager.Parameters.Select(x => x.Value));
    }

    [Fact]
    public void Single_datetime_constraint_with_nullable_property_and_datetime_struct()
    {
        DateTime value = DateTime.Now;
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.PcoeDate >= value;

        string actual = _feature.Translate(expression);

        Assert.Equal("a.PcoeDate >= @p0", actual);
        Assert.Contains(value, _parameterManager.Parameters.Select(x => x.Value));
    }

    [Fact]
    public void Single_constraint_with_field_property_value()
    {
        Command = new CommandModel() { PropertyID = 100 };
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.PropertyID == Command.PropertyID;

        string actual = _feature.Translate(expression);

        Assert.Equal("a.PropertyID = @p0", actual);
        Assert.Contains(100, _parameterManager.Parameters.Select(x => x.Value));
    }

    [Fact]
    public void Single_constraint_with_nullable_field_property_value()
    {
        Command = new CommandModel() { TaskNumber = 100 };
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.PropertyID == Command.TaskNumber;

        string actual = _feature.Translate(expression);

        Assert.Equal("a.PropertyID = @p0", actual);
        Assert.Contains(100, _parameterManager.Parameters.Select(x => x.Value));
        Assert.Contains(typeof(QueryableMod1), _feature.Columns.Select(x => x.TableType));
    }

    [Fact]
    public void Single_constraint_with_addition_and_value()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.PropertyID + 2 == 99;

        string actual = _feature.Translate(expression);

        Assert.Equal("a.PropertyID + @p0 = @p1", actual);
        Assert.Contains(2, _parameterManager.Parameters.Select(x => x.Value));
        Assert.Contains(99, _parameterManager.Parameters.Select(x => x.Value));
    }

    #endregion Single constraint statements

    #region Multiple constraint statements

    [Fact]
    public void Two_constraints_with_values()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.PropertyID == 99 & a.Address == "hello";

        string actual = _feature.Translate(expression);

        Assert.Equal("a.PropertyID = @p0 AND a.Address = @p1", actual);
        Assert.Contains(99, _parameterManager.Parameters.Select(x => x.Value));
        Assert.Contains("hello", _parameterManager.Parameters.Select(x => x.Value));
    }

    #endregion Multiple constraint statements

    #region Is Null equality constraint

    [Fact]
    public void Should_resolve_is_null_statement_from_null_equality_comparison()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.Address == null;

        string actual = _feature.Translate(expression);

        Assert.Equal("a.Address IS NULL", actual);
    }

    [Fact]
    public void Should_resolve_is_null_statement_from_not_null_equality_comparison()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.Address != null;

        string actual = _feature.Translate(expression);

        Assert.Equal("a.Address IS NOT NULL", actual);
    }

    #endregion Is Null equality constraint

    #region Condition Two tables
    [Fact]
    public void Two_constraints_from_two_separate_tables()
    {
        Expression<Func<QueryableMod1, QueryableMod2, bool>> expression = (a, b) => a.Address == null & b.EventID == 11;

        string actual = _feature.Translate(expression);

        Assert.Equal("a.Address IS NULL AND b.EventID = @p0", actual);
    }

    #endregion Condition Two tables
}