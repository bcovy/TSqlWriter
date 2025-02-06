using System.Linq.Expressions;

namespace SqlWriter.Infrastructure;

public static class MemberHelpers
{
    public static string ResolveName(this LambdaExpression expression)
    {
        return expression.Body switch
        {
            MemberExpression member => member.Member.Name,
            UnaryExpression { Operand: MemberExpression member2 } => member2.Member.Name,
            _ => ""
        };
    }
    
    public static string ResolveName(this Expression expression)
    {
        return expression switch
        {
            MemberExpression member => member.Member.Name,
            UnaryExpression { Operand: MemberExpression member2 } => member2.Member.Name,
            _ => ""
        };
    }

    public static bool ResolvesToMember(Expression expression, out MemberExpression? memberExpression)
    {
        switch (expression)
        {
            case UnaryExpression unary:
                memberExpression = unary.Operand as MemberExpression;
                return true;
            case MemberExpression member:
                memberExpression = member;
                return true;
            default:
                memberExpression = null;

                return false;
        }
    }
}
