using System.Linq.Expressions;
using System.Text;
using SqlWriter.Components.Tables;
using SqlWriter.Infrastructure;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Components.OrderBy;

public class OrderByBuilder(ITablesManager tables)
{
    private bool _addPager;
    private int _pageIndex;
    private int _pageSize;

    public bool HasConditions => Columns.Count != 0;
    public List<OrderByColumn> Columns { get; } = [];

    public string Compile()
    {
        StringBuilder result = new(" ORDER BY ");

        if (Columns.Count > 1)
        {
            foreach (var column in Columns)
                result.Append($"{column.Column} {column.Direction}, ");

            result.Length -= 2;  //Remove last comma and space. 
        }
        else
        {
            result.Append($"{Columns[0].Column} {Columns[0].Direction}");
        }

        if (!_addPager) return result.ToString();
        
        int offset = (_pageIndex - 1) * _pageSize;

        result.AppendLine().Append($" OFFSET {offset} ROWS FETCH NEXT {_pageSize} ROWS ONLY");

        return result.ToString();
    }

    public void AddPaging(int pageIndex, int pageSize)
    {
        _addPager = true;
        _pageIndex = pageIndex < 1 ? 1 : pageIndex;
        _pageSize = pageSize;
    }

    public void AddColumn(string column, string direction) => Columns.Add(new OrderByColumn(column, direction));

    public void AddColumn(LambdaExpression expression, string direction)
    {
        if (MemberHelpers.ResolvesToMember(expression.Body, out var member))
        {
            var column = GetColumnFromTable(member);

            Columns.Add(new OrderByColumn(column.ToString(), direction));
        }
        else
        {
            throw new MissingMemberException("Expecting Member Expression for Order By clause.");
        }
    }

    private ColumnModel GetColumnFromTable(MemberExpression? member)
    {
        ArgumentNullException.ThrowIfNull(member?.Expression);
        return tables.GetColumn(member.Expression.Type, member.Member.Name);
    }
}