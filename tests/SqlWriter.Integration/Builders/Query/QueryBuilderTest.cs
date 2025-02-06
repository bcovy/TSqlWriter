using SqlWriter.Integration.Fixtures;
using SqlWriter.Integration.Mocks;

namespace SqlWriter.Integration.Builders.Query;

public class QueryBuilderTest
{
    private IQueryBuilder _feature;

    public QueryBuilderTest()
    {
        _feature = SqlWriters.QueryBuilder<QueryableMod1>("a");
    }

    [Fact]
    public void Select_using_string_param_array()
    {
        _feature.Select("a.PropertyID", "a.Address");

        var actual = _feature.GetSqlStatement();

        Assert.StartsWith("SELECT a.PropertyID, a.Address FROM Table1 AS a", actual);
    }

    #region Join
    [Fact]
    public void Join_should_add_table_entity_to_statement()
    {
        _feature.Select("a.PropertyID", "a.Address")
            .Join<QueryableMod1, QueryableMod2>((a, b) => a.PropertyID == b.PropertyID, "b2");

        var actual = _feature.GetSqlStatement();

        Assert.Contains("FROM Table1 AS a", actual);
        Assert.Contains("JOIN Table2 AS b2", actual);
    }

    [Fact]
    public void Join_two_entities_should_apply_associated_tables_to_statement()
    {
        _feature.Join<QueryableMod1, QueryableMod2>((a, b) => a.PropertyID == b.PropertyID, "b2")
            .JoinLeft<QueryableMod2, QueryableMod3>((b, c) => b.PropertyID == c.PropertyID, "c3");

        var actual = _feature.GetSqlStatement();

        Assert.Contains("FROM Table1 AS a", actual);
        Assert.Contains("JOIN Table2 AS b2", actual);
        Assert.Contains("LEFT OUTER JOIN Table3 AS c3", actual);
    }

    [Fact]
    public void Join_multiple_entities_should_apply_associated_tables_to_statement()
    {
        _feature.Join<QueryableMod1, QueryableMod2>((a, b) => a.PropertyID == b.PropertyID, "b2")
            .JoinLeft<QueryableMod2, QueryableMod3>((b, c) => b.PropertyID == c.PropertyID, "c3")
            .JoinRight<QueryableMod1, QueryableMod4>((a, b) => a.PropertyID == b.PropertyID, "d4");

        var actual = _feature.GetSqlStatement();

        Assert.Contains("FROM Table1 AS a", actual);
        Assert.Contains("JOIN Table2 AS b2", actual);
        Assert.Contains("LEFT OUTER JOIN Table3 AS c3 ON b2.PropertyID = c3.PropertyID", actual);
        Assert.Contains("RIGHT OUTER JOIN Table4 AS d4 ON a.PropertyID = d4.PropertyID", actual);
    }

    #endregion Join

    #region Join CTE
    [Fact]
    public void With_should_add_cte_statement()
    {
        var cte = SqlWriters.QueryAsCte<QueryableMod1>("cteA").Select(a => new { a.PropertyID, a.Address }).Where(a => a.Address == "hello world");

        _feature.Select("a.PropertyID");
        _feature.With<QueryableMod1, int>(a => a.PropertyID, cte);
        var actual = _feature.GetSqlStatement();

        Assert.StartsWith("WITH cteA AS (SELECT a.PropertyID", actual);
        Assert.Contains("a.PropertyID, cteA.Address", actual);
        Assert.Contains("a.PropertyID = cteA.PropertyID", actual);
    }

    [Fact]
    public void With_should_add_cte_statement_with_join_expression()
    {
        var cte = SqlWriters.QueryAsCte<QueryableMod1>("cteA").Select(a => new { a.PropertyID, a.Address }).Where(a => a.Address == "hello world");

        _feature.Select("a.PropertyID");
        _feature.With<QueryableMod1, QueryableMod1>((a, b) => a.PropertyID == b.PropertyID, cte);
        var actual = _feature.GetSqlStatement();

        Assert.StartsWith("WITH cteA AS (SELECT a.PropertyID", actual);
        Assert.Contains("a.PropertyID, cteA.Address", actual);
        Assert.Contains("a.PropertyID = cteA.PropertyID", actual);
    }
    #endregion Join CTE

    #region Where

    [Fact]
    public void WhereExists_method()
    {
        _feature.WhereExists<QueryableMod2, QueryableMod1>((a, b) => a.PropertyID == b.PropertyID &  a.Address == "hello");
        var actual = _feature.GetSqlStatement();

        Assert.Contains("WHERE EXISTS (SELECT * FROM Table2 AS ext WHERE ext.PropertyID = a.PropertyID AND ext.Address = @p0)", actual);
    }

    [Fact]
    public void WhereNotExists_method()
    {
        _feature.WhereNotExists<QueryableMod2, QueryableMod1>((a, b) => a.PropertyID == b.PropertyID & a.Address == "hello");
        var actual = _feature.GetSqlStatement();

        Assert.Contains("WHERE NOT EXISTS (SELECT * FROM Table2 AS ext WHERE ext.PropertyID = a.PropertyID AND ext.Address = @p0)", actual);
    }

    [Fact]
    public void Where_using_single_entity_expression_should_apply_condition()
    {
        _feature.Where<QueryableMod1>(a => a.FirstName == "hello" & a.LastName == "world");

        var actual = _feature.GetSqlStatement();

        Assert.Contains("WHERE a.FirstName = @p0", actual);
        Assert.Equal(2, _feature.Parameters.Count());
    }

    [Fact]
    public void Where_equals_condition_should_create_parameterized_statement()
    {
        _feature.Where("a.Address", "address");

        var actual = _feature.GetSqlStatement();

        Assert.Contains("WHERE a.Address = @p0", actual);
        Assert.Single(_feature.Parameters);
    }

    [Fact]
    public void Where_equals_string_condition_should_not_be_added_when_value_is_null_and_dynamic_where_is_true()
    {
        _feature = SqlWriters.QueryBuilder<QueryableMod1>("a", true);
        _feature.Where("a.Address", string.Empty);

        var actual = _feature.GetSqlStatement();

        Assert.DoesNotContain("WHERE a.Address = @p0", actual);
        Assert.Empty(_feature.Parameters);
    }

    [Fact]
    public void Where_equals_int_condition_should_not_be_added_when_value_is_null_and_dynamic_where_is_true()
    {
        int? value = null;
        _feature = SqlWriters.QueryBuilder<QueryableMod1>("a", true);
        _feature.Where("a.Address", value);

        var actual = _feature.GetSqlStatement();

        Assert.DoesNotContain("WHERE a.Address = @p0", actual);
        Assert.Empty(_feature.Parameters);
    }

    [Fact]
    public void Where_equals_property_condition_should_not_be_added_when_value_is_null_and_dynamic_where_is_true()
    {
        _feature = SqlWriters.QueryBuilder<QueryableMod1>("a", true);
        _feature.Where("a.Address", PropertyID);

        var actual = _feature.GetSqlStatement();

        Assert.DoesNotContain("WHERE a.Address = @p0", actual);
        Assert.Empty(_feature.Parameters);
    }

    public int? PropertyID { get; set; }

    [Fact]
    public void Where_between_should_create_raw_statement_for_int_values()
    {
        _feature.WhereBetween("a.Address", 1, 99);

        var actual = _feature.GetSqlStatement();

        Assert.EndsWith("WHERE a.Address BETWEEN 1 AND 99", actual);
    }

    [Fact]
    public void Where_between_should_create_raw_single_quoted_statement_for_date_values()
    {
        var date = new DateTime(2022, 4, 20);
        _feature.WhereBetween("a.Address", date, date);

        var actual = _feature.GetSqlStatement();

        Assert.EndsWith("WHERE a.Address BETWEEN '2022-04-20 00:00:00' AND '2022-04-20 00:00:00'", actual);
    }

    [Fact]
    public void Where_not_equals_condition_should_create_parameterized_statement()
    {
        _feature.WhereNotEqual("a.Address", "address");

        var actual = _feature.GetSqlStatement();

        Assert.Contains("WHERE a.Address <> @p0", actual);
        Assert.Single(_feature.Parameters);
    }

    [Fact]
    public void Where_greater_than_condition_should_create_parameterized_statement()
    {
        _feature.WhereGreaterThan("a.Address", 99);

        var actual = _feature.GetSqlStatement();

        Assert.Contains("WHERE a.Address > @p0", actual);
        Assert.Single(_feature.Parameters);
    }

    [Fact]
    public void Where_greater_than_or_equal_condition_should_create_parameterized_statement()
    {
        _feature.WhereGreaterThanEqual("a.Address", 99);

        var actual = _feature.GetSqlStatement();

        Assert.Contains("WHERE a.Address >= @p0", actual);
        Assert.Single(_feature.Parameters);
    }

    [Fact]
    public void Where_less_than_condition_should_create_parameterized_statement()
    {
        _feature.WhereLessThan("a.Address", 99);

        var actual = _feature.GetSqlStatement();

        Assert.Contains("WHERE a.Address < @p0", actual);
        Assert.Single(_feature.Parameters);
    }

    [Fact]
    public void Where_less_than_or_equal_condition_should_create_parameterized_statement()
    {
        _feature.WhereLessThanEqual("a.Address", 99);

        var actual = _feature.GetSqlStatement();

        Assert.Contains("WHERE a.Address <= @p0", actual);
        Assert.Single(_feature.Parameters);
    }

    [Fact]
    public void Where_like_condition_should_create_parameterized_statement()
    {
        _feature.WhereLike("a.Address", "address");

        var actual = _feature.GetSqlStatement();

        Assert.Contains("WHERE a.Address LIKE @p0", actual);
        Assert.Single(_feature.Parameters);
    }

    [Fact]
    public void Where_like_condition_should_not_be_added_when_value_is_null_and_dynamic_where_is_true()
    {
        _feature = SqlWriters.QueryBuilder<QueryableMod1>("a", true);
        _feature.WhereLike("a.Address", "");

        var actual = _feature.GetSqlStatement();

        Assert.DoesNotContain("WHERE a.Address LIKE @p0", actual);
        Assert.Empty(_feature.Parameters);
    }

    [Fact]
    public void Where_not_like_condition_should_create_parameterized_statement()
    {
        _feature.WhereNotLike("a.Address", "address");

        var actual = _feature.GetSqlStatement();

        Assert.Contains("WHERE a.Address NOT LIKE @p0", actual);
        Assert.Single(_feature.Parameters);
    }

    [Fact]
    public void Where_with_multiple_conditions_should_create_parameterized_statement()
    {
        _feature.WhereLike("a.Address", "address")
            .Where<QueryableMod1>(a => Conditions.In(a.PropertyID, 1, 2));

        var actual = _feature.GetSqlStatement();

        Assert.Contains("WHERE a.Address LIKE @p0 AND a.PropertyID IN (1, 2)", actual);
        Assert.Single(_feature.Parameters);
    }

    [Fact]
    public void Where_with_or_condition_should_create_parameterized_statement()
    {
        _feature.WhereLike("a.Address", "address")
            .WhereOr()
            .Where<QueryableMod1>(a => Conditions.In(a.PropertyID, 1, 2));

        var actual = _feature.GetSqlStatement();

        Assert.Contains("WHERE a.Address LIKE @p0 OR a.PropertyID IN (1, 2)", actual);
        Assert.Single(_feature.Parameters);
    }

    [Fact]
    public void Where_subquery_should_add_condition_to_statement()
    {
        _feature.Select("a.PropertyID")
            .WhereSubquery("a.PropertyID", SubqueryMock.Subquery());

        var actual = _feature.GetSqlStatement();

        Assert.Contains("WHERE a.PropertyID = (SELECT PropertyID", actual);
    }

    #endregion Where

    #region Union

    

    #endregion
}