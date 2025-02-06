using System.Collections.Frozen;
using System.Linq.Expressions;

namespace SqlWriter.Translators;

public static class BinaryLookup
{
    private static readonly FrozenDictionary<ExpressionType, string> _stringLookup = FrozenDictionary.ToFrozenDictionary(
        [
            KeyValuePair.Create(ExpressionType.Add, "+"),
            KeyValuePair.Create(ExpressionType.AddChecked, "+"),
            KeyValuePair.Create(ExpressionType.And, "AND"),
            KeyValuePair.Create(ExpressionType.Divide, "/"),
            KeyValuePair.Create(ExpressionType.Equal, "="),
            KeyValuePair.Create(ExpressionType.GreaterThan, ">"),
            KeyValuePair.Create(ExpressionType.GreaterThanOrEqual, ">="),
            KeyValuePair.Create(ExpressionType.LessThan, "<"),
            KeyValuePair.Create(ExpressionType.LessThanOrEqual, "<="),
            KeyValuePair.Create(ExpressionType.NotEqual, "<>"),
            KeyValuePair.Create(ExpressionType.Multiply, "*"),
            KeyValuePair.Create(ExpressionType.MultiplyChecked, "*"),
            KeyValuePair.Create(ExpressionType.Subtract, "-"),
            KeyValuePair.Create(ExpressionType.SubtractChecked, "-"),
            KeyValuePair.Create(ExpressionType.Or, "OR")
        ]);

    public static string Operation(BinaryExpression node)
    {
        return _stringLookup.GetValueOrDefault(node.NodeType, "");
    }
}
