using System.Linq.Expressions;
using NSubstitute;
using SqlWriter.Components.Tables;
using SqlWriter.Interfaces.Internals;
using SqlWriter.Tests.Fixtures;
using SqlWriter.Translators;

namespace SqlWriter.Tests.Translators;

public class ConcatResolverTest
{
    private readonly ITablesManager _tables;
    private readonly TableModel _tableA;

    public ConcatResolverTest()
    {
        _tables = Substitute.For<ITablesManager>();
        _tableA = new TableModel(typeof(QueryableMod5), "a");
        _tables.GetTable(Arg.Is(typeof(QueryableMod5))).Returns(_tableA);
        _tables.GetTable(Arg.Any<Expression>()).Returns(_tableA);
    }

    [Fact]
    public void Resolve_using_property_output_alias_matches_associated_table()
    {
        Expression<Func<QueryableMod5, string>> expression = (x) => SqlFunc.Concat(x.LastName, ", ", x.FirstName);
        var call = expression.Body as MethodCallExpression;

        string actual = ConcatResolver.Resolve(call, _tables);

        Assert.Equal("CONCAT(a.LastName, ', ', a.FirstName)", actual);
    }

    [Fact]
    public void Resolve_interperates_field_value()
    {
        string fieldVal = "hello";
        Expression<Func<QueryableMod5, string>> expression = (a) => SqlFunc.Concat(fieldVal, " ", a.LastName, ", ", a.FirstName);
        var call = expression.Body as MethodCallExpression;

        string actual = ConcatResolver.Resolve(call, _tables);

        Assert.Equal("CONCAT('hello', ' ', a.LastName, ', ', a.FirstName)", actual);
    }

    [Fact]
    public void Resolve_strings_quoted_and_ints_raw()
    {
        string fieldVal = "hello";
        string fieldInt = "99";
        Expression<Func<QueryableMod5, string>> expression = (a) => SqlFunc.Concat(fieldInt, fieldVal, "66.8", a.LastName);
        var call = expression.Body as MethodCallExpression;

        string actual = ConcatResolver.Resolve(call, _tables);

        Assert.Equal("CONCAT(99, 'hello', 66.8, a.LastName)", actual);
    }

    [Fact]
    public void Resolve_inline_interpolated_string()
    {
        int fieldInt = 99;
        Expression<Func<QueryableMod5, string>> expression = (a) => SqlFunc.Concat($"hello {fieldInt} ", "world", "!!!");
        var call = expression.Body as MethodCallExpression;

        string actual = ConcatResolver.Resolve(call, _tables);

        Assert.Equal("CONCAT('hello 99 ', 'world', '!!!')", actual);
    }
}
