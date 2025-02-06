using SqlWriter.Builders.Insert;
using SqlWriter.Components.Tables;
using SqlWriter.Integration.Fixtures;

namespace SqlWriter.Integration.Builders.Insert;

public class InsertManyBuilderTest
{
    private readonly TableModel _table;
    private readonly InsertManyBuilder<QueryableMod1> _feature;

    public InsertManyBuilderTest()
    {
        _table = new TableModel(typeof(QueryableMod1), "a");
        _feature = new InsertManyBuilder<QueryableMod1>(_table);
    }

    [Fact]
    public void Init_using_default_insert_map_should_create_name_index_map_for_entity()
    {
        Assert.Equal(0, _feature.ColumnMapper["PropertyID"]);
        Assert.Equal(1, _feature.ColumnMapper["Address"]);
    }

    [Fact]
    public void Init_using_insert_column_map_should_create_name_index_map()
    {
        var sut = new InsertManyBuilder<QueryableMod1>(_table, a => new { a.Address, a.FirstName });

        Assert.Equal(2, sut.ColumnMapper.Count);
        Assert.Equal(0, sut.ColumnMapper["Address"]);
        Assert.Equal(1, sut.ColumnMapper["FirstName"]);
    }

    [Fact]
    public void SetValues_using_single_entity_should_save_insert_value_to_associated_column_map_index()
    {
        QueryableMod1 entity = new()
        {
            Address = "hello world",
            PropertyID = 99
        };

        _feature.SetValues(entity);

        var actual = Assert.Single(_feature.InsertValues);
        Assert.Equal("99", actual[0]);
        Assert.Equal("'hello world'", actual[1]);
    }

    [Fact]
    public void SetValues_should_handle_null_model_value_with_sql_NULL_replacement_value()
    {
        QueryableMod1 entity = new()
        {
            PropertyID = 99
        };

        _feature.SetValues(entity);

        var actual = Assert.Single(_feature.InsertValues);
        Assert.Equal("99", actual[0]);
        Assert.Equal("NULL", actual[1]);
    }

    [Fact]
    public void CompiledSql_should_create_insert_statement_with_associated_key_value_column_targets()
    {
        string expected = "INSERT INTO Table1 (PropertyID, Address) VALUES \n(99, 'hello world')\n ,(100, 'foo bar')";
        var sut = new InsertManyBuilder<QueryableMod1>(_table, a => new { a.PropertyID, a.Address });
        QueryableMod1 entity1 = new()
        {
            Address = "hello world",
            PropertyID = 99
        };
        QueryableMod1 entity2 = new()
        {
            Address = "foo bar",
            PropertyID = 100
        };

        sut.SetValues(entity1);
        sut.SetValues(entity2);
        var actual = sut.GetSqlStatement();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompiledSql_should_create_insert_statement_with_targeted_columns()
    {
        var sut = new InsertManyBuilder<QueryableMod1>(_table, a => new { a.PropertyID });
        string expected = "INSERT INTO Table1 (PropertyID) VALUES \n(99)\n ,(100)";
        QueryableMod1 entity1 = new()
        {
            Address = "hello world",
            PropertyID = 99
        };
        QueryableMod1 entity2 = new()
        {
            Address = "foo bar",
            PropertyID = 100
        };

        sut.SetValues(entity1);
        sut.SetValues(entity2);
        var actual = sut.GetSqlStatement();

        Assert.Equal(expected, actual);
    }
}