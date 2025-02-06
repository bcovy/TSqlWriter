using System.Linq.Expressions;
using SqlWriter.Infrastructure;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Components.GroupBy;

public class GroupByBuilder(ITablesManager tables)
{
    public bool HasConditions => Columns.Count != 0;
    public List<string> Columns { get; } = [];

    public string Compile()
    {
        return $" GROUP BY {string.Join(", ", Columns)}";
    }

    public void AddColumn(string column) => Columns.Add(column);

    public void AddColumn(LambdaExpression expression)
    {
        switch (expression.Body)
        {
            case NewExpression newexp:
                TranslateNew(newexp);
                break;
            case UnaryExpression unary:
                PushColumn(unary.Operand as MemberExpression);
                break;
            case MemberExpression member:
                PushColumn(member);
                break;
            default:
                throw new TypeAccessException("Expected Group By parameter type of New Expression.");
        }
    }

    private void TranslateNew(NewExpression expression)
    {
        foreach (var item in expression.Arguments)
        {
            if (MemberHelpers.ResolvesToMember(item, out var member))
                PushColumn(member);
        }
    }

    private void PushColumn(MemberExpression? expression)
    {
        ArgumentNullException.ThrowIfNull(expression?.Expression);
        Columns.Add(tables.GetColumn(expression.Expression.Type, expression.Member.Name).ToString());
    }
}