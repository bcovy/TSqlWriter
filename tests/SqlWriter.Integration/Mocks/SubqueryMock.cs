using NSubstitute;

namespace SqlWriter.Integration.Mocks;

public static class SubqueryMock
{
    public static ISubquery Subquery()
    {
        var subquery = Substitute.For<ISubquery>();
        subquery.GetSqlStatement().Returns("(SELECT PropertyID FROM Table2)");
        subquery.ConditionPredicate.Returns(Predicates.Equal);
        subquery.ConditionPrefix.Returns(Prefix.AND);

        return subquery;
    }

    public static Func<ISubquery> SubqueryFunc()
    {
        ISubquery subquery = Subquery();
        Func<ISubquery> func = Substitute.For<Func<ISubquery>>();
        func.Invoke().Returns(subquery);

        return func;
    }
}