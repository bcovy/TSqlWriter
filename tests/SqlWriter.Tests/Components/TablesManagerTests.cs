using System.Linq.Expressions;
using SqlWriter.Components.Tables;
using SqlWriter.Tests.Fixtures;

namespace SqlWriter.Tests.Components;

public class TablesManagerTests
{
    private readonly TablesManager _feature;

    public TablesManagerTests()
    {
        _feature = new TablesManager(typeof(QueryableMod2));
    }

    #region Init
    [Fact]
    public void Init_with_parent_table_and_generate_alias()
    {
        Assert.Equal("a", _feature.Tables[typeof(QueryableMod2)].TableAlias);
    }

    [Fact]
    public void Init_with_parent_table_as_table_variable()
    {
        TablesManager sut = new(typeof(TempTable));

        Assert.Equal("@TableTmp", sut.Tables[typeof(TempTable)].TableName);
    }

    [Fact]
    public void Init_with_parent_table_and_alias()
    {
        var feature2 = new TablesManager(typeof(QueryableMod2), "b");

        Assert.Equal("b", feature2.Tables[typeof(QueryableMod2)].TableAlias);
    }

    #endregion Init

    #region Add table
    [Fact]
    public void AddTable_entity_with_generated_alias()
    {
        _feature.AddTable<QueryableMod3>();

        Assert.Equal("a", _feature.Tables[typeof(QueryableMod2)].TableAlias);
        Assert.Equal("b", _feature.Tables[typeof(QueryableMod3)].TableAlias);
    }

    [Fact]
    public void AddTable_entity_using_fluent_method()
    {
        _feature.AddTable<QueryableMod3>();

        Assert.Equal("a", _feature.Tables[typeof(QueryableMod2)].TableAlias);
        Assert.Equal("b", _feature.Tables[typeof(QueryableMod3)].TableAlias);
    }

    [Fact]
    public void AddTable_entity_and_alias()
    {
        _feature.AddTable<QueryableMod3>("z");

        Assert.Equal("a", _feature.Tables[typeof(QueryableMod2)].TableAlias);
        Assert.Equal("z", _feature.Tables[typeof(QueryableMod3)].TableAlias);
    }

    [Fact]
    public void AddTable_should_throw_exception_when_duplicate_alias_is_used()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _feature.AddTable<QueryableMod3>("a"));
    }

    #endregion Add table

    #region Get meta data

    [Fact]
    public void GetTable_returns_table_model_for_associated_type()
    {
        var actual = _feature.GetTable(typeof(QueryableMod2));

        Assert.Equal("Table2", actual.TableName);
    }

    [Fact]
    public void GetColumn_returns_column_model_for_associated_entity()
    {
        var actual = _feature.GetColumn(typeof(QueryableMod2), "PropertyID");

        Assert.Equal("PropertyID", actual.Name);
    }

    #endregion Get meta data

    #region Add Join with Attribute PK
    [Fact]
    public void AddJoin_of_type_inner_using_entity_attribute_pk_value()
    {
        _feature.AddTable<QueryableMod3>("b");

        _feature.AddJoin<QueryableMod2, QueryableMod3>(JoinType.Inner);
        var actual = _feature.JoinInfo[0];

        Assert.Equal(JoinType.Inner, actual.JoinType);
        Assert.Equal("a", actual.TargetTableAlias);
        Assert.Equal("b", actual.TableAlias);
        Assert.Equal("Table3", actual.TableName);
        Assert.Equal("PropertyID", actual.ColumnLeft);
        Assert.Equal("PropertyID", actual.ColumnRight);
    }

    [Fact]
    public void AddJoin_for_two_tables_using_entity_attribute_pk_value()
    {
        _feature.AddTable<TaskAssignmentsMod>("b");
        _feature.AddTable<TaskMod>("c");

        _feature.AddJoin<QueryableMod2, TaskAssignmentsMod>(JoinType.Inner);
        _feature.AddJoin<TaskAssignmentsMod, TaskMod>(JoinType.Left);
        var actual1 = _feature.JoinInfo[0];
        var actual2 = _feature.JoinInfo[1];

        Assert.Equal(JoinType.Inner, actual1.JoinType);
        Assert.Equal("a", actual1.TargetTableAlias);
        Assert.Equal("b", actual1.TableAlias);
        Assert.Equal("TaskAssignments", actual1.TableName);
        Assert.Equal("PropertyID", actual1.ColumnLeft);
        Assert.Equal("PropertyID", actual1.ColumnRight);
        //task table
        Assert.Equal(JoinType.Left, actual2.JoinType);
        Assert.Equal("b", actual2.TargetTableAlias);
        Assert.Equal("c", actual2.TableAlias);
        Assert.Equal("TaskTable", actual2.TableName);
        Assert.Equal("EventID", actual2.ColumnLeft);
        Assert.Equal("EventID", actual2.ColumnRight);
    }

    [Fact]
    public void AddJoin_should_throw_exception_when_join_table_is_missing_attribute_pk_value()
    {
        _feature.AddTable<QueryableMod4>("b");

        Assert.Throws<MissingFieldException>(() => _feature.AddJoin<QueryableMod4, QueryableMod2>(JoinType.Inner));
    }
    #endregion

    #region Add Join
    [Fact]
    public void AddJoin_between_2_tables()
    {
        _feature.AddTable<QueryableMod3>("b");
        Expression<Func<QueryableMod2, QueryableMod3, bool>> expression = (a, b) => a.PropertyID == b.PropertyID;

        _feature.AddJoin(JoinType.Inner, expression);
        var actual = _feature.JoinInfo;

        Assert.Equal(JoinType.Inner, actual[0].JoinType);
        Assert.Equal("a", actual[0].TargetTableAlias);
        Assert.Equal("b", actual[0].TableAlias);
        Assert.Equal("Table3", actual[0].TableName);
        Assert.Equal("PropertyID", actual[0].ColumnLeft);
    }

    [Fact]
    public void AddJoin_2_tables_with_composite_columns()
    {
        _feature.AddTable<QueryableMod3>("b");
        Expression<Func<QueryableMod2, QueryableMod3, bool>> expression = (a, b) => a.PropertyID == b.TaskStatus & a.Address == b.Address;

        _feature.AddJoin(JoinType.Inner, expression);
        var actual = _feature.JoinInfo[0];

        Assert.Equal(JoinType.Inner, actual.JoinType);
        Assert.True(actual.IsCompositeJoin);
        Assert.Equal("a", actual.TargetTableAlias);
        Assert.Equal("b", actual.TableAlias);
        Assert.Equal("Table3", actual.TableName);
        Assert.Equal("PropertyID", actual.ColumnLeft);
        Assert.Equal("Address", actual.ColumnLeft2);
        Assert.Equal("TaskStatus", actual.ColumnRight);
        Assert.Equal("Address", actual.ColumnRight2);
    }

    [Fact]
    public void AddJoin_2_tables_with_composite_column_with_constant_numeric_value()
    {
        _feature.AddTable<QueryableMod3>("b");
        Expression<Func<QueryableMod2, QueryableMod3, bool>> expression = (a, b) => a.PropertyID == b.PropertyID & a.EventID == 12;

        _feature.AddJoin(JoinType.Inner, expression);
        var actual = _feature.JoinInfo[0];

        Assert.Equal(JoinType.Inner, actual.JoinType);
        Assert.True(actual.IsCompositeJoinConstant);
        Assert.Equal("a", actual.TargetTableAlias);
        Assert.Equal("b", actual.TableAlias);
        Assert.Equal("Table3", actual.TableName);
        Assert.Equal("PropertyID", actual.ColumnLeft);
        Assert.Equal("EventID", actual.ColumnLeft2);
        Assert.Equal("PropertyID", actual.ColumnRight);
        Assert.Equal("12", actual.ColumnRight2);
    }

    [Fact]
    public void AddJoin_2_tables_with_composite_column_with_constant_non_numeric_value()
    {
        _feature.AddTable<QueryableMod3>("b");
        Expression<Func<QueryableMod2, QueryableMod3, bool>> expression = (a, b) => a.PropertyID == b.PropertyID & a.Address == "hello";

        _feature.AddJoin(JoinType.Inner, expression);
        var actual = _feature.JoinInfo[0];

        Assert.Equal(JoinType.Inner, actual.JoinType);
        Assert.True(actual.IsCompositeJoinConstant);
        Assert.Equal("a", actual.TargetTableAlias);
        Assert.Equal("b", actual.TableAlias);
        Assert.Equal("Table3", actual.TableName);
        Assert.Equal("PropertyID", actual.ColumnLeft);
        Assert.Equal("Address", actual.ColumnLeft2);
        Assert.Equal("PropertyID", actual.ColumnRight);
        Assert.Equal("'hello'", actual.ColumnRight2);
    }

    #endregion Add Join

    #region CTE join
    [Fact]
    public void AddCteJoin_using_cte_name_and_column_string_values()
    {
        Expression<Func<QueryableMod2, int>> cteExp = (a) => a.PropertyID;

        _feature.AddCteJoin(cteExp, JoinType.Inner, "cteB");
        var actual = _feature.JoinInfo[0];

        Assert.Equal(JoinType.Inner, actual.JoinType);
        Assert.True(actual.IsCteJoin);
        Assert.Equal("a", actual.TargetTableAlias);
        Assert.Equal("cteB", actual.TableAlias);
        Assert.Equal("PropertyID", actual.ColumnLeft);
        Assert.Equal("PropertyID", actual.ColumnRight);
    }

    [Fact]
    public void AddCteJoin_using_expression()
    {
        Expression<Func<QueryableMod2, QueryableMod3, bool>> expression = (a, b) => a.PropertyID == b.PropertyID;
        var binary = expression.Body as BinaryExpression; 

        var actual = _feature.AddCteJoin(binary, JoinType.Inner, "cteB");

        Assert.Equal(JoinType.Inner, actual.JoinType);
        Assert.True(actual.IsCteJoin);
        Assert.Equal("a", actual.TargetTableAlias);
        Assert.Equal("cteB", actual.TableAlias);
        Assert.Equal("cteB", actual.TableName);
        Assert.Equal("PropertyID", actual.ColumnLeft);
        Assert.Equal("PropertyID", actual.ColumnRight);
    }

    [Fact]
    public void AddCteJoin_using_compound_join_keys_expression()
    {
        Expression<Func<QueryableMod2, QueryableMod3, bool>> expression = (a, b) => a.PropertyID == b.PropertyID & a.DescID == b.TaskStatus;
        var binary = expression.Body as BinaryExpression;

        var actual = _feature.AddCteJoin(binary, JoinType.Inner, "cteB");

        Assert.Equal(JoinType.Inner, actual.JoinType);
        Assert.True(actual.IsCteJoin);
        Assert.Equal("a", actual.TargetTableAlias);
        Assert.Equal("cteB", actual.TableAlias);
        Assert.Equal("cteB", actual.TableName);
        Assert.Equal("PropertyID", actual.ColumnLeft);
        Assert.Equal("PropertyID", actual.ColumnRight);
        Assert.Equal("DescID", actual.ColumnLeft2);
        Assert.Equal("TaskStatus", actual.ColumnRight2);
    }

    #endregion CTE join

    #region JoinWithMapper
    [Fact]
    public void JoinWithMapper_using_join_mapper_func_method()
    {
        _feature.AddTable<QueryableMod3>("b");
        _feature.JoinWithMapper(join =>
        {
            join.Inner<QueryableMod2, QueryableMod3>((a, b) => a.PropertyID == b.PropertyID);
        });
        var actual = _feature.JoinInfo;

        Assert.Equal(JoinType.Inner, actual[0].JoinType);
        Assert.Equal("a", actual[0].TargetTableAlias);
        Assert.Equal("b", actual[0].TableAlias);
        Assert.Equal("Table3", actual[0].TableName);
        Assert.Equal("PropertyID", actual[0].ColumnLeft);
    }

    [Fact]
    public void JoinWithMapper_using_entity_attribute_pk_value()
    {
        _feature.AddTable<QueryableMod3>("b");
        _feature.JoinWithMapper(join =>
        {
            join.Inner<QueryableMod2, QueryableMod3>();
        });
        var actual = _feature.JoinInfo;

        Assert.Equal(JoinType.Inner, actual[0].JoinType);
        Assert.Equal("a", actual[0].TargetTableAlias);
        Assert.Equal("b", actual[0].TableAlias);
        Assert.Equal("Table3", actual[0].TableName);
        Assert.Equal("PropertyID", actual[0].ColumnLeft);
        Assert.Equal("PropertyID", actual[0].ColumnRight);
    }

    [Fact]
    public void JoinWithMapper_using_both_entity_attribute_pk_value_and_expression()
    {
        _feature.AddTable<QueryableMod3>("b");
        _feature.AddTable<QueryableMod4>("c");
        _feature.JoinWithMapper(join =>
        {
            join.Inner<QueryableMod2, QueryableMod3>();
            join.Inner<QueryableMod2, QueryableMod4>((a, b) => a.PropertyID == b.Table4ID);
        });
        var actual1 = _feature.JoinInfo[0];
        var actual2 = _feature.JoinInfo[1];

        Assert.Equal(JoinType.Inner, actual1.JoinType);
        Assert.Equal("a", actual1.TargetTableAlias);
        Assert.Equal("b", actual1.TableAlias);
        Assert.Equal("Table3", actual1.TableName);
        Assert.Equal("PropertyID", actual1.ColumnLeft);
        //table 2
        Assert.Equal(JoinType.Inner, actual2.JoinType);
        Assert.Equal("a", actual2.TargetTableAlias);
        Assert.Equal("c", actual2.TableAlias);
        Assert.Equal("Table4", actual2.TableName);
        Assert.Equal("Table4ID", actual2.ColumnRight);
    }
    #endregion

    #region Compile Joins
    [Fact]
    public void Compile_builds_one_join()
    {
        string expected = " FROM Table2 AS a\n JOIN Table3 AS b ON a.PropertyID = b.PropertyID";
        _feature.AddTable<QueryableMod3>("b");
        Expression<Func<QueryableMod2, QueryableMod3, bool>> expression = (a, b) => a.PropertyID == b.PropertyID;
        _feature.AddJoin(JoinType.Inner, expression);

        string actual = _feature.Compile();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Compile_composite_join_with_constant_value()
    {
        string expected = " FROM Table2 AS a\n JOIN Table3 AS b ON a.PropertyID = b.PropertyID AND a.EventID = 12";
        _feature.AddTable<QueryableMod3>("b");
        Expression<Func<QueryableMod2, QueryableMod3, bool>> expression = (a, b) => a.PropertyID == b.PropertyID & a.EventID == 12;
        _feature.AddJoin(JoinType.Inner, expression);

        string actual = _feature.Compile();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Compile_composite_join_with_constant_value_as_filter_for_join_table()
    {
        string expected = " FROM Table2 AS a\n JOIN Table5 AS b ON a.PropertyID = b.PropertyID AND b.PropertyID = 12";
        _feature.AddTable<QueryableMod5>("b");
        Expression<Func<QueryableMod2, QueryableMod5, bool>> expression = (a, b) => a.PropertyID == b.PropertyID & b.PropertyID == 12;
        _feature.AddJoin(JoinType.Inner, expression);

        string actual = _feature.Compile();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Compile_composite_join_with_nullable_constant_value_as_filter_for_join_table()
    {
        string expected = " FROM Table2 AS a\n JOIN Table3 AS b ON a.PropertyID = b.PropertyID AND b.TaskStatus = 12";
        _feature.AddTable<QueryableMod3>("b");
        Expression<Func<QueryableMod2, QueryableMod3, bool>> expression = (a, b) => a.PropertyID == b.PropertyID & b.TaskStatus == 12;
        _feature.AddJoin(JoinType.Inner, expression);

        string actual = _feature.Compile();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Compile_has_one_cte_join()
    {
        string expected = " FROM Table2 AS a\n JOIN b ON a.PropertyID = b.PropertyID";
        Expression<Func<QueryableMod2, int>> cteExp = (a) => a.PropertyID;

        _feature.AddCteJoin(cteExp, JoinType.Inner, "b");
        string actual = _feature.Compile();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Compile_has_one_cte_join_using_expression()
    {
        string expected = " FROM Table2 AS a\n JOIN cteB ON a.PropertyID = cteB.PropertyID";
        Expression<Func<QueryableMod2, QueryableMod3, bool>> cteExp = (a, b) => a.PropertyID == b.PropertyID;
        var binary = cteExp.Body as BinaryExpression;

        _ = _feature.AddCteJoin(binary, JoinType.Inner, "cteB");
        string actual = _feature.Compile();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Compile_has_one_cte_join_using_composite_key_expression()
    {
        string expected = " FROM Table2 AS a\n JOIN cteB ON a.PropertyID = cteB.PropertyID AND a.DescID = cteB.TaskStatus";
        Expression<Func<QueryableMod2, QueryableMod3, bool>> cteExp = (a, b) => a.PropertyID == b.PropertyID & a.DescID == b.TaskStatus;
        var binary = cteExp.Body as BinaryExpression;

        _ = _feature.AddCteJoin(binary, JoinType.Inner, "cteB");
        string actual = _feature.Compile();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Compile_builds_joins_for_3_tables()
    {
        string expected = " FROM Table2 AS a\n JOIN Table3 AS b ON a.PropertyID = b.PropertyID LEFT OUTER JOIN Table4 AS c ON a.PropertyID = c.PropertyID";
        _feature.AddTable<QueryableMod3>("b");
        _feature.AddTable<QueryableMod4>("c");
        Expression<Func<QueryableMod2, QueryableMod3, bool>> join1 = (a, b) => a.PropertyID == b.PropertyID;
        Expression<Func<QueryableMod2, QueryableMod4, bool>> join2 = (a, b) => a.PropertyID == b.PropertyID;

        _feature.AddJoin(JoinType.Inner, join1);
        _feature.AddJoin(JoinType.Left, join2);

        string actual = _feature.Compile();

        Assert.Equal(expected, actual);
    }

    #endregion Compile Joins
}