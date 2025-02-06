using SqlWriter.Infrastructure;
using System.Linq.Expressions;
using SqlWriter.Tests.Fixtures;

namespace SqlWriter.Tests.Infrastructure;

public class MemberHelpersTest
{
    public string testField = "hello world";
    
    [Fact]
    public void ResolvesToMember_returns_true_when_expression_is_a_property()
    {
        QueryableMod1 model = new();
        MemberExpression expression = Expression.Property(Expression.Constant(model), "Address");

        Assert.True(MemberHelpers.ResolvesToMember(expression, out MemberExpression result));
        Assert.NotNull(result);
    }

    [Fact]
    public void ResolvesToMember_returns_true_when_expression_is_a_nullable_property()
    {
        QueryableMod2 model = new();
        MemberExpression expression = Expression.Property(Expression.Constant(model), "YesNo");

        Assert.True(MemberHelpers.ResolvesToMember(expression, out MemberExpression result));
        Assert.NotNull(result);
    }

    [Fact]
    public void ResolvesToMember_returns_true_when_expression_is_a_field()
    {
        var expression = Expression.Field(Expression.Constant(this), "testField");

        Assert.True(MemberHelpers.ResolvesToMember(expression, out MemberExpression result));
        Assert.NotNull(result);
    }

    [Fact]
    public void ResolvesToMember_returns_false_when_expression_not_an_accessable_type()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => Conditions.In(a.Address, "a", "b");
        var call = expression.Body as MethodCallExpression;

        Assert.False(MemberHelpers.ResolvesToMember(call, out MemberExpression result));
        Assert.Null(result);
    }

    [Fact]
    public void ResolvesToMember_returns_false_when_expression_value_is_a_struct()
    {
        ConstantExpression expression = Expression.Constant(DateTime.Now, typeof(DateTime));

        Assert.False(MemberHelpers.ResolvesToMember(expression, out MemberExpression result));
        Assert.Null(result);
    }
}
