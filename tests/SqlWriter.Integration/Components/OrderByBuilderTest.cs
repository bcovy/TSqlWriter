using System.Linq.Expressions;
using NSubstitute;
using SqlWriter.Components.OrderBy;
using SqlWriter.Components.Tables;
using SqlWriter.Integration.Fixtures;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Integration.Components;

public class OrderByBuilderTest
{
    private readonly OrderByBuilder _feature;
    private readonly ITablesManager _tables;

    public OrderByBuilderTest()
    {
        _tables = Substitute.For<ITablesManager>();
        _feature = new OrderByBuilder(_tables);
    }

    [Fact]
    public void Add_string_column()
    {
        _feature.AddColumn("a.PropertyID", "DESC");

        Assert.Contains("a.PropertyID", _feature.Columns.Select(x => x.Column));
    }

    [Fact]
    public void Add_single_property_column()
    {
        ColumnModel column = new("PropertyID", typeof(QueryableMod1), "a");
        _tables.GetColumn(Arg.Any<Type>(), Arg.Any<string>()).Returns(column);
        Expression<Func<QueryableMod1, int>> expression = (a) => a.PropertyID;

        _feature.AddColumn(expression, "DESC");

        Assert.Contains("a.PropertyID", _feature.Columns.Select(x => x.Column));
    }

    [Fact]
    public void Add_single_nullable_type_property_column()
    {
        ColumnModel column = new("PcoeDate", typeof(QueryableMod1), "a");
        _tables.GetColumn(Arg.Any<Type>(), Arg.Any<string>()).Returns(column);
        Expression<Func<QueryableMod1, DateTime?>> expression = (a) => a.PcoeDate;

        _feature.AddColumn(expression, "ASC");

        Assert.Contains("a.PcoeDate", _feature.Columns.Select(x => x.Column));
    }

    [Fact]
    public void Add_two_property_columns()
    {
        ColumnModel column1 = new("PropertyID", typeof(QueryableMod1), "a");
        ColumnModel column2 = new("Address", typeof(QueryableMod1), "a");
        _tables.GetColumn(Arg.Any<Type>(), Arg.Is("DESC")).Returns(column1);
        _tables.GetColumn(Arg.Any<Type>(), Arg.Is("ASC")).Returns(column2);
        Expression<Func<QueryableMod1, int>> expression = (a) => a.PropertyID;
        Expression<Func<QueryableMod1, String>> expression2 = (a) => a.Address;

        _feature.AddColumn(expression, "DESC");
        _feature.AddColumn(expression2, "asc");

        Assert.True(_feature.Columns.Count == 2);
    }

    [Fact]
    public void Compile_order_by_from_two_property_columns()
    {
        ColumnModel column1 = new("Address", typeof(QueryableMod1), "a");
        ColumnModel column2 = new("PropertyID", typeof(QueryableMod1), "a");
        _tables.GetColumn(Arg.Any<Type>(), Arg.Is("Address")).Returns(column1);
        _tables.GetColumn(Arg.Any<Type>(), Arg.Is("PropertyID")).Returns(column2);
        Expression<Func<QueryableMod1, string>> expression1 = (a) => a.Address;
        Expression<Func<QueryableMod1, int>> expression2 = (a) => a.PropertyID;

        _feature.AddColumn(expression1, "DESC");
        _feature.AddColumn(expression2, "ASC");
        string actual = _feature.Compile();

        Assert.Equal(" ORDER BY a.Address DESC, a.PropertyID ASC", actual);
    }

    [Fact]
    public void Compile_order_by_with_paging()
    {
#if OSX
        string expected = " ORDER BY a.Address DESC, a.PropertyID ASC\n OFFSET 0 ROWS FETCH NEXT 25 ROWS ONLY";
#else
        string expected = " ORDER BY a.Address DESC, a.PropertyID ASC\r\n OFFSET 0 ROWS FETCH NEXT 25 ROWS ONLY";
#endif
        ColumnModel column1 = new("Address", typeof(QueryableMod1), "a");
        ColumnModel column2 = new("PropertyID", typeof(QueryableMod1), "a");
        _tables.GetColumn(Arg.Any<Type>(), Arg.Is("Address")).Returns(column1);
        _tables.GetColumn(Arg.Any<Type>(), Arg.Is("PropertyID")).Returns(column2);
        Expression<Func<QueryableMod1, string>> expression1 = (a) => a.Address;
        Expression<Func<QueryableMod1, int>> expression2 = (a) => a.PropertyID;

        _feature.AddColumn(expression1, "DESC");
        _feature.AddColumn(expression2, "ASC");
        _feature.AddPaging(1, 25);
        string actual = _feature.Compile();

        Assert.Equal(expected, actual);
    }
}