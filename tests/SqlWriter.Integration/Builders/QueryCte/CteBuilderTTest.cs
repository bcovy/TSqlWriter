using System.Linq.Expressions;
using SqlWriter.Integration.Fixtures;
using SqlWriter.Integration.Mocks;

namespace SqlWriter.Integration.Builders.QueryCte;

public class CteBuilderTTest
{
    private readonly ICte<QueryableMod1> _feature;

    public CteBuilderTTest()
    {
        _feature = SqlWriters.QueryAsCte<QueryableMod1>("cteA", false);
    }

    #region Select
    [Fact]
    public void Select_one_column()
    {
        string expected = "SELECT a.PropertyID FROM Table1 AS a";
        Expression<Func<QueryableMod1, object>> expression = a => new { a.PropertyID };

        _feature.Select(expression);
        string actual = _feature.CompileStatement();

        Assert.StartsWith(expected, actual);
    }

    [Fact]
    public void Select_default_of_select_all()
    {
        string expected = "SELECT a.PropertyID, a.Address";

        var actual = _feature.CompileStatement();

        Assert.StartsWith(expected, actual);
    }

    [Fact]
    public void Select_columns_using_raw_string_of_params()
    {
        _feature.Select("a.column1", "b.column2 AS [SomeColumn]");

        string actual = _feature.CompileStatement();

        Assert.StartsWith("SELECT a.column1, b.column2 AS [SomeColumn]", actual);
    }

    [Fact]
    public void Select_columns_using_literal_raw_string_of_params()
    {
        _feature.Select(@"a.column""1""", "b.column2 AS [SomeColumn]");

        string actual = _feature.CompileStatement();

        Assert.StartsWith(@"SELECT a.column""1"", b.column2 AS [SomeColumn]", actual);
    }

    #endregion Select

    #region  Where
    [Fact]
    public void Where_with_single_expression()
    {
        string expected = "SELECT a.PropertyID FROM Table1 AS a\n WHERE a.PropertyID = @cteA0";
        Expression<Func<QueryableMod1, object>> expression = a => new { a.PropertyID };
        Expression<Func<QueryableMod1, bool>> where = a => a.PropertyID == 99;

        _feature.Select(expression);
        _feature.Where(where);
        string actual = _feature.CompileStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Where_with_two_expressions()
    {
        string expected = "SELECT a.PropertyID FROM Table1 AS a\n WHERE a.PropertyID = @cteA0 AND a.Address = @cteA1";
        Expression<Func<QueryableMod1, object>> expression = a => new { a.PropertyID };
        Expression<Func<QueryableMod1, bool>> where = a => a.PropertyID == 99 & a.Address == "hello";

        _feature.Select(expression);
        _feature.Where(where);
        string actual = _feature.CompileStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Where_with_two_expressions_using_or_prefix()
    {
        string expected = "SELECT a.PropertyID FROM Table1 AS a\n WHERE a.PropertyID = @cteA0 OR a.PropertyID = @cteA1";
        Expression<Func<QueryableMod1, object>> expression = a => new { a.PropertyID };
        Expression<Func<QueryableMod1, bool>> where = a => a.PropertyID == 99;

        _feature.Select(expression);
        _feature.Where(where);
        _feature.WhereOr();
        _feature.Where(where);
        string actual = _feature.CompileStatement();

        Assert.Equal(expected, actual);
    }

    #endregion Where

    #region WhereExists
    [Fact]
    public void WhereExists_method()
    {
        string expected = "SELECT a.PropertyID FROM Table1 AS a\n WHERE EXISTS (SELECT * FROM Table2 AS ext WHERE ext.PropertyID = a.PropertyID AND ext.Address = @cteA0)";
        Expression<Func<QueryableMod1, object>> expression = a => new { a.PropertyID };

        _feature.Select(expression);
        _feature.WhereExists<QueryableMod2>((a, b) => a.PropertyID == b.PropertyID & a.Address == "hello");

        string actual = _feature.CompileStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void WhereNotExists_method()
    {
        string expected = "SELECT a.PropertyID FROM Table1 AS a\n WHERE NOT EXISTS (SELECT * FROM Table2 AS ext WHERE ext.PropertyID = a.PropertyID AND ext.Address = @cteA0)";
        Expression<Func<QueryableMod1, object>> expression = a => new { a.PropertyID };

        _feature.Select(expression);
        _feature.WhereNotExists<QueryableMod2>((a, b) => a.PropertyID == b.PropertyID & a.Address == "hello");

        string actual = _feature.CompileStatement();

        Assert.Equal(expected, actual);
    }

    #endregion WhereExists

    #region Where Subquery
    [Fact]
    public void Where_subquery_condition_for_expression_target_column()
    {
        Expression<Func<QueryableMod1, int>> expression = (a) => a.PropertyID;

        _feature.WhereSubquery(expression, SubqueryMock.Subquery());
        string actual = _feature.CompileStatement();

        Assert.EndsWith(" WHERE a.PropertyID = (SELECT PropertyID FROM Table2)", actual);
    }

    #endregion Where Subquery

    #region Group by
    [Fact]
    public void Group_by_one_column()
    {
        string expected = "SELECT a.PropertyID FROM Table1 AS a\n GROUP BY a.FirstName";
        Expression<Func<QueryableMod1, object>> expression = a => new { a.PropertyID };
        Expression<Func<QueryableMod1, string>> groupBy = a => a.FirstName;

        _feature.Select(expression);
        _feature.GroupBy(groupBy);
        string actual = _feature.CompileStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Group_by_two_columns()
    {
        string expected = "SELECT a.PropertyID FROM Table1 AS a\n GROUP BY a.FirstName, a.LastName";
        Expression<Func<QueryableMod1, object>> expression = a => new { a.PropertyID };
        Expression<Func<QueryableMod1, object>> groupBy = a => new { a.FirstName, a.LastName };

        _feature.Select(expression);
        _feature.GroupBy(groupBy);
        string actual = _feature.CompileStatement();

        Assert.Equal(expected, actual);
    }

    #endregion Group by

    #region Having
    [Fact]
    public void Having_clause_using_sql_condition_method()
    {
        string expected = "SELECT a.PropertyID FROM Table1 AS a\n WHERE a.PropertyID = @cteA0 HAVING COUNT(*) > 2";
        Expression<Func<QueryableMod1, object>> expression = a => new { a.PropertyID };
        Expression<Func<QueryableMod1, bool>> where = a => a.PropertyID == 99;
        Expression<Func<QueryableMod1, bool>> having = a => SqlFunc.Count() > 2;

        _feature.Select(expression);
        _feature.Where(where);
        _feature.Having(having);
        string actual = _feature.CompileStatement();

        Assert.Equal(expected, actual);
    }

    #endregion Having

    #region Order by
    [Fact]
    public void Order_by_one_column()
    {
        string expected = "SELECT a.PropertyID FROM Table1 AS a\n ORDER BY a.FirstName ASC";
        Expression<Func<QueryableMod1, object>> expression = a => new { a.PropertyID };
        Expression<Func<QueryableMod1, string>> orderBy = a => a.FirstName;

        _feature.Select(expression);
        _feature.OrderByAsc(orderBy);
        string actual = _feature.CompileStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Order_by_two_columns()
    {
        string expected = "SELECT a.PropertyID FROM Table1 AS a\n ORDER BY a.FirstName ASC, a.LastName DESC";
        Expression<Func<QueryableMod1, object>> expression = a => new { a.PropertyID };
        Expression<Func<QueryableMod1, string>> orderBy1 = a => a.FirstName;
        Expression<Func<QueryableMod1, string>> orderBy2 = a => a.LastName;

        _feature.Select(expression);
        _feature.OrderByAsc(orderBy1);
        _feature.OrderByDesc(orderBy2);
        string actual = _feature.CompileStatement();

        Assert.Equal(expected, actual);
    }

    #endregion Order by
}
