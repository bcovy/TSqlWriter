using System.Linq.Expressions;
using NSubstitute;
using SqlWriter.Components.GroupBy;
using SqlWriter.Components.Tables;
using SqlWriter.Integration.Fixtures;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Integration.Components;

public class GroupByBuilderTest
{
    private readonly GroupByBuilder _feature;
    private readonly ITablesManager _tables;

    public GroupByBuilderTest()
    {
        _tables = Substitute.For<ITablesManager>();
        _feature = new GroupByBuilder(_tables);
    }

    [Fact]
    public void Add_single_property_column_string()
    {
        _feature.AddColumn("SomeColumn");

        Assert.Contains("SomeColumn", _feature.Columns);
    }

    [Fact]
    public void Add_single_property_column()
    {
        ColumnModel column = new("PropertyID", typeof(QueryableMod1), "a");
        _tables.GetColumn(Arg.Any<Type>(), Arg.Any<string>()).Returns(column);
        Expression<Func<QueryableMod1, int>> expression = (a) => a.PropertyID;

        _feature.AddColumn(expression);

        Assert.Contains("a.PropertyID", _feature.Columns);
    }

    [Fact]
    public void Add_single_nullable_type_property_column()
    {
        ColumnModel column = new("PcoeDate", typeof(QueryableMod1), "a");
        _tables.GetColumn(Arg.Any<Type>(), Arg.Any<string>()).Returns(column);
        Expression<Func<QueryableMod1, DateTime?>> expression = (a) => a.PcoeDate;

        _feature.AddColumn(expression);

        Assert.Contains("a.PcoeDate", _feature.Columns);
    }

    [Fact]
    public void Add_columns_using_new_expression()
    {
        ColumnModel column1 = new("Address", typeof(QueryableMod1), "a");
        ColumnModel column2 = new("PropertyID", typeof(QueryableMod1), "a");
        _tables.GetColumn(Arg.Any<Type>(), Arg.Is("Address")).Returns(column1);
        _tables.GetColumn(Arg.Any<Type>(), Arg.Is("PropertyID")).Returns(column2);
        Expression<Func<QueryableMod1, object>> expression = (a) => new { a.Address, a.PropertyID };

        _feature.AddColumn(expression);

        Assert.Contains("a.Address", _feature.Columns);
        Assert.Contains("a.PropertyID", _feature.Columns);
    }

    [Fact]
    public void Compile_single_property_column()
    {
        ColumnModel column = new("PropertyID", typeof(QueryableMod1), "a");
        _tables.GetColumn(Arg.Any<Type>(), Arg.Any<string>()).Returns(column);
        Expression<Func<QueryableMod1, int>> expression = (a) => a.PropertyID;

        _feature.AddColumn(expression);
        string actual = _feature.Compile();

        Assert.Equal(" GROUP BY a.PropertyID", actual);
    }

    [Fact]
    public void Compile_multiple_property_column()
    {
        ColumnModel column1 = new("Address", typeof(QueryableMod1), "a");
        ColumnModel column2 = new("PropertyID", typeof(QueryableMod1), "a");
        _tables.GetColumn(Arg.Any<Type>(), Arg.Is("Address")).Returns(column1);
        _tables.GetColumn(Arg.Any<Type>(), Arg.Is("PropertyID")).Returns(column2);
        Expression<Func<QueryableMod1, object>> expression = (a) => new { a.Address, a.PropertyID };

        _feature.AddColumn(expression);
        string actual = _feature.Compile();

        Assert.Equal(" GROUP BY a.Address, a.PropertyID", actual);
    }
}