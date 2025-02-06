using System.Linq.Expressions;
using System.Reflection;

namespace SqlWriter.Infrastructure;

public static class ExpressionHelpers
{
    private static readonly HashSet<ExpressionType> ExpressionTypes =
        [
            ExpressionType.Constant, ExpressionType.New, 
            ExpressionType.NewArrayInit, ExpressionType.MemberAccess,
            ExpressionType.Call, ExpressionType.ArrayIndex
        ];

    private static bool CanReturnValue(this Expression? node)
    {
        return node != null && ExpressionTypes.Contains(node.NodeType);
    }
    /// <summary>
    /// Returns <see langword="true"/> if the expression belongs to an expression parameter.
    /// Method helps to identify if expression is used to represent a SQL column.
    /// </summary>
    /// <param name="node">Expression.</param>
    /// <returns><see langword="true"/> if the expression belongs to an expression parameter.</returns>
    public static bool BelongsToParameter(this Expression node)
    {
        return node switch
        {
            MemberExpression { Expression: null } => false,
            MemberExpression member => member.Expression.NodeType == ExpressionType.Parameter,
            UnaryExpression { Operand.NodeType: ExpressionType.MemberAccess } unary when
                unary.Operand is MemberExpression member2 => member2.NodeType == ExpressionType.Parameter,
            _ => false
        };
    }
    /// <summary>
    /// Returns true if the expression is a constant or unary type, and has no value.
    /// </summary>
    /// <param name="node">Expression.</param>
    /// <returns>True if the expression is a constant or unary type, and has no value.</returns>
    public static bool IsNullUnaryOrConstant(this Expression node)
    {
        return node switch
        {
            ConstantExpression constant => constant.Value == null,
            UnaryExpression { Operand: ConstantExpression constant2 } => constant2.Value == null,
            _ => false
        };
    }
    /// <summary>
    /// Returns a value from a <see cref="MemberExpression"/> if expression is a type that
    /// can return a value (i.e. a property or field).
    /// </summary>
    /// <param name="node">Member expression.</param>
    /// <returns>Value of member expression.</returns>
    public static object? GetValue(this MemberExpression node)
    {
        object? parentValue = null;

        if (node.Expression != null || node.Expression.CanReturnValue())
            parentValue = node.Expression?.GetValue();
        // Try/Catch used to deal with anonymous objects with no easily identifiable type. 
        try
        {
            return node.Member.MemberType switch
            {
                MemberTypes.Property => ((PropertyInfo)node.Member).GetValue(parentValue, null),
                MemberTypes.Field => ((FieldInfo)node.Member).GetValue(parentValue),
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }
    /// <summary>
    /// Returns a value from an expression by invoking its method.
    /// </summary>
    /// <param name="node">Expression.</param>
    /// <returns>Value of invoked method.</returns>
    public static object? GetValue(this MethodCallExpression node)
    {
        var args = node.Arguments.Select(a => a.GetValue()).ToArray();
        object? parent = null;

        if (node.Object != null && node.Object.CanReturnValue())
            parent = node.Object.GetValue();

        return node.Method.Invoke(parent, args);
    }
    /// <summary>
    /// Returns the value of an expression if it's a property, field, constant or method call.
    /// </summary>
    /// <remarks>
    /// Uses recursion to get the result.
    /// </remarks>
    /// <param name="node">Expression.</param>
    /// <returns>Value of an expression if it's a property, field, constant or method call.</returns>
    public static object? GetValue(this Expression node)
    {
        switch (node)
        {
            case ConstantExpression constant:
                return constant.Value;
            case MemberExpression member:
                return member.GetValue();
            case UnaryExpression unary:
                return GetConvertValue(unary);
            case NewExpression newExpression:
                var args = newExpression.Arguments.Select(a => a.GetValue()).ToArray();

                return newExpression.Constructor == null ? Activator.CreateInstance(node.Type, args) : newExpression.Constructor.Invoke(args);
            case MethodCallExpression method:
                return method.GetValue();
            default:
                return null;
        }
    }

    private static Type GetGenericArgument(this Type tp, int index = 0)
    {
        if (!tp.GetTypeInfo().IsGenericType)
            throw new InvalidOperationException("Provided type is not generic");

        var typeArgs = tp.GenericTypeArguments;

        return typeArgs[index];
    }

    private static bool IsNullable(this Type type)
    {
        return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    private static bool IsEnumType(this Type type) => type.IsEnum || (type.IsNullable() && type.GetGenericArgument().IsEnum);

    private static object? GetConvertValue(UnaryExpression node)
    {
        var value = node.Operand.GetValue();

        if (!node.Type.IsEnumType())
            return value == null ? value : Convert.ChangeType(value, node.Type.IsNullable() ? node.Type.GetGenericArgument() : node.Type);
        
        if (node.Type.IsEnum && value is not null)
            return value is string st ? Enum.Parse(node.Type, st) : Enum.ToObject(node.Type, value);
        //nullable
        if (value != null)
            return _ = value is string st ? Enum.Parse(node.Type.GetGenericArgument(), st) : Enum.ToObject(node.Type.GetGenericArgument(), value);

        return value == null 
            ? value 
            : Convert.ChangeType(value, node.Type.IsNullable() ? node.Type.GetGenericArgument() : node.Type);
    }
}