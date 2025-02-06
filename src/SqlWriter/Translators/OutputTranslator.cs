using SqlWriter.Infrastructure;
using System.Linq.Expressions;

namespace SqlWriter.Translators;

public class OutputTranslator
{
    private string _actionName = "Inserted";
    private List<(string Output, string Into)> _columns = [];

    public string Visit(LambdaExpression expression, string actionName = "Inserted")
    {
        _columns = [];
        _actionName = actionName;

        if (expression.Body is MemberInitExpression mi)
            VisitMemberInt(mi);
        else
            throw new InvalidCastException("Expecting lambda expression of type MemberInitExpression.");

        string outputTable = expression.Body.Type.IsGenericType
            ? TableNameHelper.GetName(expression.Body.Type.GetGenericArguments()[0])
            : TableNameHelper.GetName(expression.Body.Type);
        string output = string.Join(", ", _columns.Select(x => x.Output));
        string targets = string.Join(", ", _columns.Select(x => x.Into));

        return $"OUTPUT {output} INTO {outputTable} ({targets})";
    }

    private void VisitMemberInt(MemberInitExpression memberInt)
    {
        foreach (var item in memberInt.Bindings)
        {
            if (item is not MemberAssignment assign) 
                continue;
            
            if (assign.Expression is MemberExpression member)
            {
                //base case.
                _columns.Add(($"{_actionName}.{member.Member.Name}", item.Member.Name));
            }
            else if (assign.Expression is MemberInitExpression memberInt2)
            {
                _actionName = item.Member.Name;
                VisitMemberInt(memberInt2);
            }
        }
    }
}
