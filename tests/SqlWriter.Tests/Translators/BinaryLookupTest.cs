using System.Linq.Expressions;
using SqlWriter.Tests.Fixtures;
using SqlWriter.Translators;

namespace SqlWriter.Tests.Translators;

public class BinaryLookupTest
{
    [Fact]
    public void Operation_equals()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.PropertyID == 99;
        BinaryExpression binary = (BinaryExpression)expression.Body;

        string actual = BinaryLookup.Operation(binary);

        Assert.Equal("=", actual);
    }

    [Fact]
    public void Operation_greater_than()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.PropertyID > 99;
        BinaryExpression binary = (BinaryExpression)expression.Body;

        string actual = BinaryLookup.Operation(binary);

        Assert.Equal(">", actual);
    }

    [Fact]
    public void Operation_not_equals()
    {
        Expression<Func<QueryableMod1, bool>> expression = (a) => a.PropertyID != 99;
        BinaryExpression binary = (BinaryExpression)expression.Body;

        string actual = BinaryLookup.Operation(binary);

        Assert.Equal("<>", actual);
    }
}
