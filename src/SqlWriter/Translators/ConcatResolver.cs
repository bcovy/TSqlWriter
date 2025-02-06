using SqlWriter.Infrastructure;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Translators;

public static partial class ConcatResolver
{
    /// <summary>
    /// Translates the arguments of a <see cref="SqlFunc.Concat(string[])"/> method into a string result of 'CONCAT(argument1, argument2, ...)'
    /// </summary>
    /// <param name="expression">Expression containing <see cref="SqlFunc.Concat(string[])"/> method.</param>
    /// <param name="tableManager">Table manager.</param>
    /// <returns>String SQL function CONCAT(argument1, argument2, ...)</returns>
    public static string Resolve(MethodCallExpression expression, ITablesManager tableManager)
    {
        var node = expression.Arguments[0] as NewArrayExpression;
        string[] values = new string[node!.Expressions.Count];

        for (int i = 0; i < node.Expressions.Count; i++)
        {
            if (node.Expressions[i] is ConstantExpression constant)
            {
                values[i] = IsNumeric(constant.Value) ? $"{constant.Value}" : $"'{constant.Value}'";
                continue;
            }

            if (node.Expressions[i] is MemberExpression member)
            {
                if (member.Member.MemberType == MemberTypes.Property)
                {
                    var table = tableManager.GetTable(member);

                    values[i] = $"{table.TableAlias}.{member.Member.Name}";  //assumed field name from database.
                }
                else
                {
                    var value = member.GetValue();

                    values[i] = IsNumeric(value) ? $"{value}" : $"'{value}'";
                }

                continue;
            }

            if (node.Expressions[i] is MethodCallExpression call)
                values[i] = $"'{call.GetValue()}'"; //should be a string interpolation function.
        }

        return $"CONCAT({string.Join(", ", values)})";
    }
    //1. starts with optional minus sign (-?)
    //2. has one or more digits ([0-9]+)
    //3. has an optional decimal followed by one or more digits (?:\.[0-9]+)?$)
    [GeneratedRegex(@"^-?[0-9]+(?:\.[0-9]+)?$")]
    private static partial Regex IsDigitRegex();

    private static bool IsNumeric(object? value)
    {
        if (value is null)
            return false;
        
        return value is string input && IsDigitRegex().IsMatch(input);
    }
}
