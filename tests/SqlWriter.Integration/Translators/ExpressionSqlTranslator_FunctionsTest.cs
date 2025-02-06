using System.Data;
using System.Linq.Expressions;
using NSubstitute;
using SqlWriter.Components.Parameters;
using SqlWriter.Integration.Fixtures;
using SqlWriter.Interfaces.Internals;
using SqlWriter.Translators;

namespace SqlWriter.Integration.Translators;

public class ExpressionSqlTranslator_FunctionsTest
{
    private readonly ExpressionSqlTranslator _feature;
    private readonly ParameterManager _parameterManager;

    public ExpressionSqlTranslator_FunctionsTest()
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

    #region SqlConditions In

    [Fact]
    public void In_condition_for_int_types()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.In(a.PropertyID, 1, 2, 3);

        string actual = _feature.Translate(expression);

        Assert.Equal("a.PropertyID IN (1, 2, 3)", actual);
    }

    [Fact]
    public void In_condition_for_int_types_with_no_table_alias()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.In(a.PropertyID, 1, 2, 3);

        string actual = _feature.TranslateWithoutAlias(expression);

        Assert.Equal("PropertyID IN (1, 2, 3)", actual);
    }

    [Fact]
    public void NotIn_condition_for_int_types()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.NotIn(a.PropertyID, 1, 2, 3);

        string actual = _feature.Translate(expression);

        Assert.Equal("a.PropertyID NOT IN (1, 2, 3)", actual);
    }
    #endregion SqlConditions In

    #region SqlConditions IsNull

    [Fact]
    public void IsNull_condition_for_string_type()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.IsNull(a.Address);

        string actual = _feature.Translate(expression);

        Assert.Equal("a.Address IS NULL", actual);
    }

    [Fact]
    public void IsNotNull_condition_for_string_type()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.IsNotNull(a.Address);

        string actual = _feature.Translate(expression);

        Assert.Equal("a.Address IS NOT NULL", actual);
    }

    [Fact]
    public void IsNull_condition_for_string_type_with_no_table_alias()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.IsNull(a.Address);

        string actual = _feature.TranslateWithoutAlias(expression);

        Assert.Equal("Address IS NULL", actual);
    }

    #endregion SqlConditions IsNull

    #region SqlConditions Group

    [Fact]
    public void Group_condition_for_multiple_statements()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.Group(a.PropertyID == 12 | a.LastName == "Hello world");

        string actual = _feature.Translate(expression);

        Assert.Equal("(a.PropertyID = @p0 OR a.LastName = @p1)", actual);
    }

    #endregion SqlConditions Group

    #region SqlConditions Between

    [Fact]
    public void Between_condition_for_int_types()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.Between(a.PropertyID, 1, 2);

        string actual = _feature.Translate(expression);

        Assert.Equal("a.PropertyID BETWEEN @p0 AND @p1", actual);
        Assert.Equal(2, _parameterManager.Parameters.Count);
    }

    #endregion SqlConditions Between

    #region SqlConditions Like

    [Fact]
    public void Like_condition_for_contains()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.Like(a.Address, "hello");

        string actual = _feature.Translate(expression);
        var actualParam = _parameterManager.Parameters.FirstOrDefault();

        Assert.Equal("a.Address LIKE @p0", actual);
        Assert.Equal("%hello%", actualParam.Value);
    }

    [Fact]
    public void Like_condition_for_contains_should_resolve_value_from_field_value()
    {
        string value = "hello";
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.Like(a.Address, value);

        string actual = _feature.Translate(expression);
        var actualParam = _parameterManager.Parameters.FirstOrDefault();

        Assert.Equal("a.Address LIKE @p0", actual);
        Assert.Equal("%hello%", actualParam.Value);
    }

    [Fact]
    public void Like_condition_for_contains_should_resolve_value_from_property_field_value()
    {
        var command = new CommandModel { Name = "hello" };
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.Like(a.Address, command.Name);

        string actual = _feature.Translate(expression);
        var actualParam = _parameterManager.Parameters.FirstOrDefault();

        Assert.Equal("a.Address LIKE @p0", actual);
        Assert.Equal("%hello%", actualParam.Value);
    }

    [Fact]
    public void Like_condition_for_starts()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.Like(a.Address, "hello", "starts");

        string actual = _feature.Translate(expression);
        var actualParam = _parameterManager.Parameters.FirstOrDefault();

        Assert.Equal("a.Address LIKE @p0", actual);
        Assert.Equal("hello%", actualParam.Value);
    }

    #endregion SqlConditions Like

    #region SqlConditions ParameterizedSql

    [Fact]
    public void ParameterizedSql_condition_using_statement_only()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.ParameterizedSql("DATEDIFF(DAY, a.PcoeDate, GETDATE()) >= @PcoeDate");

        string actual = _feature.Translate(expression);

        Assert.Equal("DATEDIFF(DAY, a.PcoeDate, GETDATE()) >= @PcoeDate", actual);
        Assert.Empty(_parameterManager.Parameters);
    }

    [Fact]
    public void ParameterizedSql_condition_using_property_field_and_value()
    {
        var date = DateTime.Now;
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.ParameterizedSql("DATEDIFF(DAY, a.PcoeDate, GETDATE()) >= @PcoeDate", a.PcoeDate, date);

        string actual = _feature.Translate(expression);
        var actualParam = _parameterManager.Parameters.FirstOrDefault();

        Assert.Equal("DATEDIFF(DAY, a.PcoeDate, GETDATE()) >= @PcoeDate", actual);
        Assert.Equal("@PcoeDate", actualParam.ParameterName);
    }

    [Fact]
    public void ParameterizedSql_condition_using_property_fields()
    {
        var command = new CommandModel() { TaskNumber = 99 };
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.ParameterizedSql("DATEDIFF(DAY, a.PcoeDate, GETDATE()) >= @PropertyID", a.PropertyID, command.TaskNumber);

        string actual = _feature.Translate(expression);
        var actualParam = _parameterManager.Parameters.FirstOrDefault();

        Assert.Equal("DATEDIFF(DAY, a.PcoeDate, GETDATE()) >= @PropertyID", actual);
        Assert.Equal("@PropertyID", actualParam.ParameterName);
    }

    #endregion SqlConditions ParameterizedSql

    #region SqlFunctions Avg
    [Fact]
    public void Function_avg_with_property()
    {
        Expression<Func<QueryableMod1, int>> expression = (a) => SqlFunc.Average(a.PropertyID);

        string actual = _feature.Translate(expression);

        Assert.Equal("AVG(a.PropertyID)", actual);
    }

    [Fact]
    public void Function_avg_with_nested_operator()
    {
        Expression<Func<QueryableMod1, int>> expression = (a) => SqlFunc.Average(a.PropertyID * 1.0M);

        string actual = _feature.Translate(expression);

        Assert.Equal("AVG(a.PropertyID * 1.0)", actual);
    }

    #endregion SqlFunctions Avg

    #region SqlFunctions Concat

    [Fact]
    public void Function_concat_strings_quoted_ints_raw_and_property_name()
    {
        string fieldVal = "hello";
        string fieldInt = "99";
        Expression<Func<QueryableMod1, string>> expression = (a) => SqlFunc.Concat(fieldInt, fieldVal, "66.8", a.LastName);

        string actual = _feature.Translate(expression);

        Assert.Equal("CONCAT(99, 'hello', 66.8, a.LastName)", actual);
    }

    #endregion SqlFunctions Concat

    #region SqlFunctions Cast
    [Fact]
    public void Function_cast_with_property_type_int()
    {
        Expression<Func<QueryableMod1, int>> expression = (a) => SqlFunc.Cast(a.PropertyID, "SMALLINT");

        string actual = _feature.Translate(expression);

        Assert.Equal("CAST(a.PropertyID AS SMALLINT)", actual);
    }

    [Fact]
    public void Function_cast_with_property_type_datetime()
    {
        Expression<Func<QueryableMod1, DateTime?>> expression = (a) => SqlFunc.Cast(a.PcoeDate, "DATE");

        string actual = _feature.Translate(expression);

        Assert.Equal("CAST(a.PcoeDate AS DATE)", actual);
    }

    [Fact]
    public void Function_cast_with_nested_sql_function()
    {
        Expression<Func<QueryableMod1, int>> expression = (a) => SqlFunc.Cast(SqlFunc.Count(), "FLOAT");

        string actual = _feature.Translate(expression);

        Assert.Equal("CAST(COUNT(*) AS FLOAT)", actual);
    }

    #endregion SqlFunctions Cast

    #region SqlFunctions EoMonth
    [Fact]
    public void Function_eomonth_from_property()
    {
        Expression<Func<QueryableMod1, DateTime?>> expression = (a) => SqlFunc.EoMonth(a.PcoeDate);

        string actual = _feature.Translate(expression);

        Assert.Equal("EOMONTH(a.PcoeDate)", actual);
    }

    [Fact]
    public void Function_eomonth_from_datetime_struct()
    {
        Expression<Func<QueryableMod1, DateTime>> expression = (a) => SqlFunc.EoMonth(DateTime.Now);

        string actual = _feature.Translate(expression);

        Assert.Equal("EOMONTH(@p0)", actual);
        var result = Assert.Single(_parameterManager.Parameters);
        Assert.Equal(SqlDbType.DateTime, result.SqlDataType);
    }

    #endregion SqlFunctions EoMonth

    #region SqlFunctions DateDiff
    [Fact]
    public void Function_date_diff_with_property_field_and_datetime_struct()
    {
        Expression<Func<QueryableMod1, int>> expression = (a) => SqlFunc.DateDiff("DAY", a.PcoeDate, DateTime.Now);

        string actual = _feature.Translate(expression);

        Assert.Equal("DATEDIFF(DAY, a.PcoeDate, @p0)", actual);
        var result = Assert.Single(_parameterManager.Parameters);
        Assert.Equal(SqlDbType.DateTime, result.SqlDataType);
    }

    [Fact]
    public void Function_date_diff_with_additional_operator()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => SqlFunc.DateDiff("DAY", a.PcoeDate, DateTime.Now) >= 121;

        string actual = _feature.Translate(expression);
        var result = _parameterManager.Parameters.FirstOrDefault();

        Assert.Equal("DATEDIFF(DAY, a.PcoeDate, @p0) >= @p1", actual);
        Assert.Equal(SqlDbType.DateTime, result.SqlDataType);
    }

    #endregion SqlFunctions DateDiff

    #region SqlFunctions IIF
    [Fact]
    public void Function_iif_with_constant_string_inputs()
    {
        Expression<Func<QueryableMod1, string>> expression = (a) => SqlFunc.IIF(a.PropertyID == 12, "yes", "no");

        string actual = _feature.Translate(expression);

        Assert.Equal("IIF(a.PropertyID = 12, 'yes', 'no')", actual);
    }

    [Fact]
    public void Function_iif_with_constants_datetime_and_int_inputs_should_translate_raw_values()
    {
        var date = DateTime.Now;
        string expected = $"IIF(a.PropertyID = 12, '{date:yyyy-MM-dd HH:mm:ss}', a.PcoeDate)";
        Expression<Func<QueryableMod1, DateTime?>> expression = (a) => SqlFunc.IIF(a.PropertyID == 12, date, a.PcoeDate);

        string actual = _feature.Translate(expression);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Function_iif_with_constant_int_inputs()
    {
        Expression<Func<QueryableMod1, int>> expression = (a) => SqlFunc.IIF(a.PropertyID == 12, 99, 100);

        string actual = _feature.Translate(expression);

        Assert.Equal("IIF(a.PropertyID = 12, 99, 100)", actual);
    }

    [Fact]
    public void Function_iif_with_constant_int_inputs_and_operators()
    {
        Expression<Func<QueryableMod2, int>> expression = (a) => SqlFunc.IIF(a.PropertyID / SqlFunc.Cast(a.EventID, "FLOAT") >= 0.9M, 1, 2);

        string actual = _feature.Translate(expression);

        Assert.Equal("IIF(b.PropertyID / CAST(b.EventID AS FLOAT) >= 0.9, 1, 2)", actual);
    }

    [Fact]
    public void Function_iif_with_addition_condition_and_constant_inputs()
    {
        Expression<Func<QueryableMod1, int>> expression = (a) => SqlFunc.IIF(a.PropertyID + 1 == 12, 99, 100);

        string actual = _feature.Translate(expression);

        Assert.Equal("IIF(a.PropertyID + 1 = 12, 99, 100)", actual);
    }

    [Fact]
    public void Function_iif_with_field_property_inputs()
    {
        Command = new CommandModel() { TaskNumber = 100 };
        Expression<Func<QueryableMod1, int?>> expression = (a) => SqlFunc.IIF(a.PropertyID == 12, Command.TaskNumber, 101);

        string actual = _feature.Translate(expression);

        Assert.Equal("IIF(a.PropertyID = 12, 100, 101)", actual);
    }

    [Fact]
    public void Function_iif_uses_complex_or_condition()
    {
        var date = DateTime.Now;
        string expected = $"IIF(a.PcoeDate > '{date:yyyy-MM-dd HH:mm:ss}' OR a.PcoeDate IS NULL, 1, 0)";
        Expression<Func<QueryableMod1, int>> expression = (a) => SqlFunc.IIF(a.PcoeDate > date | a.PcoeDate == null, 1, 0);

        string actual = _feature.Translate(expression);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Function_iif_nested_with_additional_iif_function()
    {
        Expression<Func<QueryableMod2, int>> expression = (a) => SqlFunc.IIF(a.PropertyID == 12, 3, SqlFunc.IIF(a.EventID + 12 > 99, 2, 1));

        string actual = _feature.Translate(expression);

        Assert.Equal("IIF(b.PropertyID = 12, 3, IIF(b.EventID + 12 > 99, 2, 1))", actual);
    }

    #endregion SqlFunctions IIF

    #region SqlFunctions Max/Min

    [Fact]
    public void Function_max_with_property()
    {
        Expression<Func<QueryableMod1, int>> expression = (a) => SqlFunc.Max(a.PcoeDate);

        string actual = _feature.Translate(expression);

        Assert.Equal("MAX(a.PcoeDate)", actual);
    }

    [Fact]
    public void Function_min_with_property()
    {
        Expression<Func<QueryableMod1, int>> expression = (a) => SqlFunc.Min(a.PcoeDate);

        string actual = _feature.Translate(expression);

        Assert.Equal("MIN(a.PcoeDate)", actual);
    }

    #endregion SqlFunctions Max/Min

    #region SqlFunctions Round

    [Fact]
    public void Function_round_from_field_property()
    {
        Expression<Func<QueryableMod1, int>> expression = (a) => SqlFunc.Round(a.PropertyID, 1);

        string actual = _feature.Translate(expression);

        Assert.Equal("ROUND(a.PropertyID, 1)", actual);
    }

    [Fact]
    public void Function_round_with_nested_function()
    {
        Expression<Func<QueryableMod1, int>> expression = a => SqlFunc.Round(SqlFunc.Average(a.PropertyID * 1.0M), 0);

        string actual = _feature.Translate(expression);

        Assert.Equal("ROUND(AVG(a.PropertyID * 1.0), 0)", actual);
    }

    #endregion SqlFunctions Round

    #region SqlFunctions Count

    [Fact]
    public void Function_count_all_resolve()
    {
        Expression<Func<QueryableMod1, int>> expression = (a) => SqlFunc.Count();

        string actual = _feature.Translate(expression);

        Assert.Equal("COUNT(*)", actual);
    }

    [Fact]
    public void Function_count_all_resolve_with_operator()
    {
        Expression<Func<QueryableMod1, decimal>> expression = (a) => SqlFunc.Count() * 1.0M;

        string actual = _feature.Translate(expression);

        Assert.Contains(SqlDbType.Decimal, _parameterManager.Parameters.Select(x => x.SqlDataType));
        Assert.Equal("COUNT(*) * @p0", actual);
    }

    [Fact]
    public void Function_count_member_expression_resolve()
    {
        Expression<Func<QueryableMod1, int>> expression = (a) => SqlFunc.Count(a.PropertyID);

        string actual = _feature.Translate(expression);

        Assert.Equal("COUNT(a.PropertyID)", actual);
    }

    [Fact]
    public void Function_count_member_resolve_with_operator()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => SqlFunc.Count(a.PropertyID) > 4;

        string actual = _feature.Translate(expression, doNotParameterizeValues: true);

        Assert.Equal("COUNT(a.PropertyID) > 4", actual);
    }

    #endregion SqlFunctions Count

    #region SqlFunctions Sum

    [Fact]
    public void Function_sum_resolve_member_expression()
    {
        Expression<Func<QueryableMod1, int>> expression = (a) => SqlFunc.Sum(a.PropertyID);

        string actual = _feature.Translate(expression);

        Assert.Equal("SUM(a.PropertyID)", actual);
    }

    [Fact]
    public void Function_sum_resolve_with_operator()
    {
        Expression<Func<QueryableMod1, int>> expression = (a) => SqlFunc.Sum(a.PropertyID) * 125;

        string actual = _feature.Translate(expression);

        Assert.Equal("SUM(a.PropertyID) * @p0", actual);
        Assert.Contains(125, _parameterManager.Parameters.Select(x => x.Value));
    }

    [Fact]
    public void Function_sum_resolve_with_nested_iif_function()
    {
        Expression<Func<QueryableMod1, int>> expression = (a) => SqlFunc.Sum(SqlFunc.IIF(a.Address == "hello", 1, 2));

        string actual = _feature.Translate(expression);

        Assert.Equal("SUM(IIF(a.Address = 'hello', 1, 2))", actual);
    }

    [Fact]
    public void Function_sum_resolve_with_multiple_functions()
    {
        Expression<Func<QueryableMod1, int>> expression = (a) => SqlFunc.Sum(SqlFunc.IIF(a.Address == "hello", 1, 2)) / SqlFunc.Cast(SqlFunc.Count(), "FLOAT");

        string actual = _feature.Translate(expression);

        Assert.Equal("SUM(IIF(a.Address = 'hello', 1, 2)) / CAST(COUNT(*) AS FLOAT)", actual);
    }

    #endregion SqlFunctions Sum

    #region SqlFunctions RawSql

    [Fact]
    public void Function_rawsql_string()
    {
        Expression<Func<QueryableMod4, bool>> expression = (a) => SqlFunc.RawSql("'hello world'");

        string actual = _feature.Translate(expression);

        Assert.Equal("'hello world'", actual);
    }

    [Fact]
    public void Function_rawsql_from_interpolated_string()
    {
        string expected = $"'{DateTime.Now:yyyy-MM-dd}'";
        Expression<Func<QueryableMod4, bool>> expression = (a) => SqlFunc.RawSql($"'{DateTime.Now:yyyy-MM-dd}'");

        string actual = _feature.Translate(expression);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Function_rawsql_function_from_field_property_value()
    {
        Command = new CommandModel() { PropertyID = 2, Name = "Hello world!!" };
        Expression<Func<QueryableMod4, bool>> expression = (a) => SqlFunc.RawSql(Command.Name);

        string actual = _feature.Translate(expression);

        Assert.Equal(Command.Name, actual);
    }

    #endregion SqlFunctions RawSql
}