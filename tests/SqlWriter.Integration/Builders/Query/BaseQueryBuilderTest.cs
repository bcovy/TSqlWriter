using System.Linq.Expressions;
using SqlWriter.Builders.Query;
using SqlWriter.Components.Tables;
using SqlWriter.Integration.Fixtures;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Integration.Builders.Query;

public class BaseQueryFixture(ITablesManager tables, IParameterManager parameterManager, string parameterPrefix = "p", string unionStatement = null) 
    : BaseQueryBuilder(tables, parameterManager, parameterPrefix, unionStatement);

public class BaseQueryBuilderTest
{
    private readonly BaseQueryFixture _feature;
    private readonly TablesManager _tables;

    public BaseQueryBuilderTest()
    {
        _tables = new(typeof(QueryableMod1), "a");
        _feature = new(_tables, ManagersFixture.GetParameterManager());
    }

    #region Select

    [Fact]
    public void Select_one_column_using_expression()
    {
        Expression<Func<QueryableMod1, object>> expression = a => new { a.PropertyID };

        _feature.SelectBase(expression);

        Assert.Single(_feature.SelectBuilder.Columns);
    }

    [Fact]
    public void Select_columns_using_raw_string_of_params()
    {
        _feature.SelectBase("a.column1", "b.column2 AS [SomeColumn]");

        Assert.Equal(2, _feature.SelectBuilder.Columns.Count);
    }

    [Fact]
    public void Select_subquery_column()
    {
        string expected = "(SELECT PropertyID FROM Table2 WHERE PropertyID = @sub10) AS [sub1]";

        _feature.SelectSubqueryBase("sub1", () =>
        {
            return SqlWriters.Subquery<QueryableMod2>()
                .Select(a => a.PropertyID)
                .Where(a => a.PropertyID == 11);
        });
        var actual = _feature.SelectBuilder.Columns.First();

        Assert.Equal(expected, actual.ColumnValue);
        Assert.Single(_feature.GetParameters);
    }

    #endregion

    #region Union
    [Fact]
    public void Union_returns_iunion_object_with_union_clause()
    {
        string expected = "SELECT a.Address, a.PropertyID FROM Table1 AS a\nUNION\n";
        Expression<Func<QueryableMod1, object>> expression = a => new { a.Address, a.PropertyID };
        
        _feature.SelectBase(expression);
        var actual = _feature.Union();

        Assert.Equal(expected, actual.SqlStatement);
    }
    
    [Fact]
    public void Union_all_returns_iunion_object_with_union_clause()
    {
        string expected = "SELECT a.Address, a.PropertyID FROM Table1 AS a\nUNION ALL\n";
        Expression<Func<QueryableMod1, object>> expression = a => new { a.Address, a.PropertyID };
        
        _feature.SelectBase(expression);
        var actual = _feature.UnionAll();

        Assert.Equal(expected, actual.SqlStatement);
    }
    
    [Fact]
    public void Union_condition_compiles_parameter_statement_and_get_sql_statement()
    {
        string expected = "SELECT a.Address, a.PropertyID FROM Table1 AS a\nUNION\nSELECT a.Address, a.PropertyID FROM Table1 AS a\n";
        
        _feature.SelectBase("a.Address, a.PropertyID");
        var union = _feature.Union();
        var sut = new BaseQueryFixture(_tables, union.ParameterManager, union.ParameterPrefix, union.SqlStatement);
        Expression<Func<QueryableMod1, object>> expression = a => new { a.Address, a.PropertyID };
        sut.SelectBase(expression);

        string actual = sut.GetSqlStatement();

        Assert.Equal(expected, actual);
    }
    

    #endregion

    #region With CTE

    [Fact]
    public void With_should_add_cte_statement_excluding_cte_join_column_from_parent_projection_results()
    {
        var cte = SqlWriters.QueryAsCte<QueryableMod5>(includeCteJoinColumn: false)
            .Select(a => new { a.Address, a.PropertyID });
        Expression<Func<QueryableMod1, int>> with = a => a.PropertyID;

        _feature.WithCte(with, cte);

        Assert.NotNull(_feature.CteColumns);
        Assert.Contains(("cteA", "Address"), _feature.CteColumns);
        Assert.DoesNotContain(("cteA", "PropertyID"), _feature.CteColumns);
    }

    [Fact]
    public void With_should_add_cte_statement_and_include_cte_join_column_from_parent_projection_results()
    {
        var cte = SqlWriters.QueryAsCte<QueryableMod5>(includeCteJoinColumn: true)
            .Select(a => new { a.Address, a.PropertyID });
        Expression<Func<QueryableMod1, int>> with = a => a.PropertyID;

        _feature.WithCte(with, cte);

        Assert.NotNull(_feature.CteColumns);
        Assert.Contains(("cteA", "Address"), _feature.CteColumns);
        Assert.Contains(("cteA", "PropertyID"), _feature.CteColumns);
    }

    [Fact]
    public void With_should_add_cte_statement_and_project_columns_into_select_statement_when_composite_join_is_used()
    {
        var cte = SqlWriters.QueryAsCte<QueryableMod5>("cteA", stopColumnsProjection: false)
            .Select(a => new { a.Address, a.PropertyID, a.FirstName, a.LastName })
            .Where(a => a.Address == "hello world");
        Expression<Func<QueryableMod1, QueryableMod5, bool>> with = (a, b) =>
            a.PropertyID == b.PropertyID & a.Address == b.FirstName;

        _feature.WithCte(with, cte);

        Assert.NotNull(_feature.CteColumns);
        Assert.Equal(2, _feature.CteColumns.Count);
        Assert.Contains(("cteA", "Address"), _feature.CteColumns);
        Assert.Contains(("cteA", "LastName"), _feature.CteColumns);
    }

    [Fact]
    public void
        With_should_add_cte_statement_and_project_columns_and_join_keys_into_select_statement_when_composite_join_is_used()
    {
        var cte = SqlWriters
            .QueryAsCte<QueryableMod5>("cteA", stopColumnsProjection: false, includeCteJoinColumn: true)
            .Select(a => new { a.Address, a.PropertyID, a.FirstName, a.LastName })
            .Where(a => a.Address == "hello world");
        Expression<Func<QueryableMod1, QueryableMod5, bool>> with = (a, b) =>
            a.PropertyID == b.PropertyID & a.Address == b.FirstName;

        _feature.WithCte(with, cte);

        Assert.Equal(4, _feature.CteColumns.Count);
        Assert.Contains(("cteA", "PropertyID"), _feature.CteColumns);
        Assert.Contains(("cteA", "Address"), _feature.CteColumns);
        Assert.Contains(("cteA", "FirstName"), _feature.CteColumns);
        Assert.Contains(("cteA", "LastName"), _feature.CteColumns);
    }

    [Fact]
    public void With_should_add_cte_statement_and_not_project_columns_into_parent_query()
    {
        var cte = SqlWriters.QueryAsCte<QueryableMod5>("cteA", stopColumnsProjection: true)
            .Select(a => new { a.Address, a.PropertyID });
        Expression<Func<QueryableMod1, int>> with = a => a.PropertyID;

        _feature.WithCte(with, cte);

        Assert.NotNull(_feature.CteColumns);
        Assert.Empty(_feature.CteColumns);
    }

    [Fact]
    public void Compile_cte_should_produce_single_cte_clause()
    {
        string expected =
            "WITH cteA AS (SELECT a.PropertyID, a.Address FROM Table1 AS a\n WHERE a.Address = @cteA0)\n";
        var cte = SqlWriters.QueryAsCte<QueryableMod1>("cteA").Select(a => new { a.PropertyID, a.Address })
            .Where(a => a.Address == "hello world");
        Expression<Func<QueryableMod1, int>> with = a => a.PropertyID;

        _feature.WithCte(with, cte);
        string actual = _feature.CompileCte();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Compile_statements_should_produce_two_cte_clauses()
    {
        string expected =
            "WITH cteA AS (SELECT a.PropertyID, a.Address FROM Table1 AS a\n WHERE a.Address = @cteA0), \ncteB AS (SELECT a.PropertyID, a.Address FROM Table1 AS a\n WHERE a.Address = @cteB0)\n";
        var cte1 = SqlWriters.QueryAsCte<QueryableMod1>("cteA").Select(a => new { a.PropertyID, a.Address })
            .Where(a => a.Address == "hello world");
        var cte2 = SqlWriters.QueryAsCte<QueryableMod1>("cteB")
            .Select(a => new { a.PropertyID, a.Address })
            .Where(a => a.Address == "hello world");
        Expression<Func<QueryableMod1, int>> with = a => a.PropertyID;

        _feature.WithCte(with, cte1);
        _feature.WithCte(with, cte2);
        string actual = _feature.CompileCte();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void With_should_add_cte_statement()
    {
        string expected =
            "WITH cteA AS (SELECT a.PropertyID, a.Address FROM Table5 AS a\n WHERE a.Address = @cteA0)\nSELECT a.PropertyID, cteA.Address FROM Table1 AS a\n JOIN cteA ON a.PropertyID = cteA.PropertyID";
        ICteStatement cte = SqlWriters.QueryAsCte<QueryableMod5>("cteA")
            .Select(a => new { a.PropertyID, a.Address })
            .Where(a => a.Address == "hello world");
        Expression<Func<QueryableMod1, object>> expression = a => new { a.PropertyID };
        Expression<Func<QueryableMod1, int>> with = a => a.PropertyID;

        _feature.SelectBuilder.TranslateExpression(expression);
        _feature.WithCte(with, cte);
        string actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void With_should_add_cte_statement_with_unique_parameter_names()
    {
        ICteStatement cte = SqlWriters.QueryAsCte<QueryableMod5>("cteA")
            .Select(a => new { a.Address, a.PropertyID })
            .Where(a => a.Address == "hello world");
        Expression<Func<QueryableMod1, object>> expression = a => new { a.PropertyID };
        Expression<Func<QueryableMod1, int>> with = a => a.PropertyID;
        Expression<Func<QueryableMod1, bool>> where = a => a.PropertyID == 99;

        _feature.SelectBuilder.TranslateExpression(expression);
        _feature.WithCte(with, cte);
        _feature.WhereBase(where);
        var actual = _feature.GetSqlStatement();
        var parameters = _feature.GetParameters.Keys;

        Assert.Contains("cteA0", parameters);
        Assert.Contains("p0", parameters);
    }

    #endregion With CTE

    #region With CTE composite

    [Fact]
    public void With_should_add_cte_statement_using_expression_as_join_columns()
    {
        string expected =
            "WITH cteA AS (SELECT a.PropertyID, a.Address FROM Table5 AS a\n WHERE a.Address = @cteA0)\nSELECT a.PropertyID, cteA.Address FROM Table1 AS a\n JOIN cteA ON a.PropertyID = cteA.PropertyID";
        ICteStatement cte = SqlWriters.QueryAsCte<QueryableMod5>("cteA")
            .Select(a => new { a.PropertyID, a.Address }).Where(a => a.Address == "hello world");
        Expression<Func<QueryableMod1, object>> expression = a => new { a.PropertyID };
        Expression<Func<QueryableMod1, QueryableMod5, bool>> with = (a, b) => a.PropertyID == b.PropertyID;

        _feature.SelectBase(expression);
        _feature.WithCte(with, cte);
        string actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void With_should_add_cte_statement_using_expression_as_composite_join_columns()
    {
        string expected =
            "WITH cteA AS (SELECT a.PropertyID, a.Address, a.FirstName FROM Table5 AS a\n WHERE a.Address = @cteA0)\nSELECT a.PropertyID, cteA.FirstName FROM Table1 AS a\n JOIN cteA ON a.PropertyID = cteA.PropertyID AND a.Address = cteA.Address";
        ICteStatement cte = SqlWriters.QueryAsCte<QueryableMod5>("cteA")
            .Select(a => new { a.PropertyID, a.Address, a.FirstName }).Where(a => a.Address == "hello world");
        Expression<Func<QueryableMod1, object>> expression = a => new { a.PropertyID };
        Expression<Func<QueryableMod1, QueryableMod5, bool>> with = (a, b) =>
            a.PropertyID == b.PropertyID & a.Address == b.Address;

        _feature.SelectBase(expression);
        _feature.WithCte(with, cte);
        string actual = _feature.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    #endregion With CTE composite

    #region WhereExists

    [Fact]
    public void WhereExists_method()
    {
        string expected =
            "EXISTS (SELECT * FROM Table2 AS ext WHERE ext.PropertyID = a.PropertyID AND ext.Address = @p0)";

        _feature.WhereExistsBase<QueryableMod2, QueryableMod1>((a, b) =>
            a.PropertyID == b.PropertyID & a.Address == "hello");

        Assert.Contains(expected, _feature.WhereBuilder.Conditions.Select(x => x.Item2));
    }

    [Fact]
    public void WhereNotExists_method()
    {
        string expected =
            "NOT EXISTS (SELECT * FROM Table2 AS ext WHERE ext.PropertyID = a.PropertyID AND ext.Address = @p0)";

        _feature.WhereExistsBase<QueryableMod2, QueryableMod1>(
            (a, b) => a.PropertyID == b.PropertyID & a.Address == "hello", true);

        Assert.Contains(expected, _feature.WhereBuilder.Conditions.Select(x => x.Item2));
    }

    #endregion WhereExists

    #region Group by

    [Fact]
    public void Group_by_one_column()
    {
        Expression<Func<QueryableMod1, string>> groupBy = a => a.FirstName;

        _feature.GroupByBase(groupBy);

        Assert.Single(_feature.GroupByBuilder.Columns);
    }

    [Fact]
    public void Group_by_two_columns()
    {
        Expression<Func<QueryableMod1, object>> groupBy = a => new { a.FirstName, a.LastName };

        _feature.GroupByBase(groupBy);

        Assert.Equal(2, _feature.GroupByBuilder.Columns.Count);
    }

    #endregion Group by

    #region Having

    [Fact]
    public void Having_clause_using_sql_condition_method()
    {
        Expression<Func<QueryableMod1, bool>> having = a => SqlFunc.Count() > 2;

        _feature.HavingBase(having);

        Assert.Equal("COUNT(*) > 2", _feature.HavingCondition);
    }

    #endregion Having

    #region Order by

    [Fact]
    public void Order_by_one_column()
    {
        Expression<Func<QueryableMod1, string>> orderBy = a => a.FirstName;

        _feature.OrderByBase(orderBy, "ASC");

        Assert.Single(_feature.OrderByBuilder.Columns);
    }

    [Fact]
    public void Order_by_two_columns()
    {
        Expression<Func<QueryableMod1, string>> orderBy1 = a => a.FirstName;
        Expression<Func<QueryableMod1, string>> orderBy2 = a => a.LastName;

        _feature.OrderByBase(orderBy1, "ASC");
        _feature.OrderByBase(orderBy2, "DESC");

        Assert.Equal(2, _feature.OrderByBuilder.Columns.Count);
    }

    #endregion Order by
}