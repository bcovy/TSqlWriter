using SqlWriter.Infrastructure;
using System.Linq.Expressions;
using System.Text;

namespace SqlWriter.Translators;

public static class ConditionInResolver
{
    public static string Resolve(MethodCallExpression expression)
    {
        StringBuilder sb = new StringBuilder(expression.Method.Name == "NotIn" ? " NOT IN" : " IN").Append(" (");

        if (expression.Arguments[1] is NewArrayExpression na)
        {
            foreach (var t in na.Expressions)
            {
                if (t is ConstantExpression constant)
                {
                    if (TypeHelper.IsNumericType(na.Type.GetElementType()))
                        sb.Append($"{constant.Value}, ");
                    else
                        sb.Append($"'{constant.Value}', ");
                }
                else
                {
                    sb.Append($"'{t.GetValue()}', ");
                }
            }
            //Remove last comma and space.
            sb.Length -= 2;
            sb.Append(')');
        }
        else if (expression.Arguments[1] is MemberExpression member)
        {
            var elementType = member.Type.GetElementType();

            if (TypeHelper.IsNumericType(elementType))
            {
                var objectMember = Expression.Convert(member, typeof(int[]));
                var getterLambda = Expression.Lambda<Func<int[]>>(objectMember);
                var getter = getterLambda.Compile();
                var value = getter();
                sb.Append($"{string.Join(", ", value)})");
            }
            else if (elementType == typeof(DateOnly))
            {
                var objectMember = Expression.Convert(member, typeof(DateOnly[]));
                var getterLambda = Expression.Lambda<Func<DateOnly[]>>(objectMember);
                var getter = getterLambda.Compile();
                var value = getter();
                string result = $"{string.Join(", ", value.Select(x => "'" + x + "'"))})";
                sb.Append(result);
            }
            else
            {
                var objectMember = Expression.Convert(member, typeof(string[]));
                var getterLambda = Expression.Lambda<Func<string[]>>(objectMember);
                var getter = getterLambda.Compile();
                var value = getter();
                string result = $"{string.Join(", ", value.Select(x => "'" + x + "'"))})";
                sb.Append(result);
            }
        }

        return sb.ToString();
    }
}