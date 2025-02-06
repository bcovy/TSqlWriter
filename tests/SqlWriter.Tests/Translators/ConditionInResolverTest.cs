using SqlWriter.Translators;
using System;
using System.Linq.Expressions;
using SqlWriter.Tests.Fixtures;
using Xunit;

namespace SqlWriter.Tests.Translators;

public class ConditionInResolverTest
{
    [Fact]
    public void Should_resolve_in_expression_for_int_type()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.In(a.PropertyID, 1, 2);
        var call = expression.Body as MethodCallExpression;

        string actual = ConditionInResolver.Resolve(call);

        Assert.Equal(" IN (1, 2)", actual);
    }

    [Fact]
    public void Should_resolve_in_expression_for_string_type()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.In(a.Address, "a", "b");
        var call = expression.Body as MethodCallExpression;

        string actual = ConditionInResolver.Resolve(call);

        Assert.Equal(" IN ('a', 'b')", actual);
    }

    [Fact]
    public void Should_resolve_in_expression_for_date_type()
    {
        DateOnly date1 = new(2024, 10, 1);
        DateOnly date2 = new(2024, 10, 2);
        Expression<Func<QueryableMod5, bool>> expression = (a) => Conditions.In(a.CloseDate, date1, date2);
        var call = expression.Body as MethodCallExpression;

        string actual = ConditionInResolver.Resolve(call);

        Assert.Equal(" IN ('10/1/2024', '10/2/2024')", actual);
    }

    [Fact]
    public void Should_resolve_in_expression_for_nullable_date_type()
    {
        DateOnly date1 = new(2024, 10, 1);
        DateOnly date2 = new(2024, 10, 2);
        Expression<Func<QueryableMod5, bool>> expression = (a) => Conditions.In(a.PcoeDate, date1, date2);
        var call = expression.Body as MethodCallExpression;

        string actual = ConditionInResolver.Resolve(call);

        Assert.Equal(" IN ('10/1/2024', '10/2/2024')", actual);
    }

    [Fact]
    public void Should_resolve_in_expression_for_int_member_expression()
    {
        int[] expected = [1, 2];
        var command = new CommandModel2() { PropertyID = 99, EventIDs = expected };
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.In(a.PropertyID, command.EventIDs);
        var call = expression.Body as MethodCallExpression;

        string actual = ConditionInResolver.Resolve(call);

        Assert.Equal(" IN (1, 2)", actual);
    }

    [Fact]
    public void Should_resolve_in_expression_for_string_member_expression()
    {
        var command = new CommandModel2() { Strings = ["hello", "world"] };
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.In(a.Address, command.Strings);
        var call = expression.Body as MethodCallExpression;

        string actual = ConditionInResolver.Resolve(call);

        Assert.Equal(" IN ('hello', 'world')", actual);
    }

    [Fact]
    public void Should_resolve_in_expression_for_date_member_expression()
    {
        var command = new CommandModel2() { Dates = [new(2024, 10, 1), new(2024, 10, 2)] };
        Expression<Func<QueryableMod5, bool>> expression = (a) => Conditions.In(a.CloseDate, command.Dates);
        var call = expression.Body as MethodCallExpression;

        string actual = ConditionInResolver.Resolve(call);

        Assert.Equal(" IN ('10/1/2024', '10/2/2024')", actual);
    }

    public CommandModel2 Command { get; set; }

    public class CommandModel2
    {
        public int PropertyID { get; set; }
        public string Address { get; set; }
        public DateOnly SomeDate { get; set; }
        public int[] EventIDs { get; set; }
        public string[] Strings { get; set; }
        public DateOnly[] Dates { get; set; }
    }
}