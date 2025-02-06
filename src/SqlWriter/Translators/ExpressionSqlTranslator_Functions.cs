using SqlWriter.Infrastructure;
using System.Linq.Expressions;
using SqlWriter.Components.Parameters;
using SqlWriter.Components.Tables;

namespace SqlWriter.Translators;

public partial class ExpressionSqlTranslator
{
    #region Conditions

    private void ResolveBetween(MethodCallExpression expression)
    {
        CreateColumn(expression.Arguments[0]);

        _sb.Append(" BETWEEN ");
        CreateParameter(expression.Arguments[1].GetValue());
        _sb.Append(" AND ");
        CreateParameter(expression.Arguments[2].GetValue());
    }

    private void ResolveGroup(MethodCallExpression expression)
    {
        _sb.Append('(');
        Visit(expression.Arguments[0]);
        _sb.Append(')');
    }

    private void ResolveLike(MethodCallExpression expression)
    {
        var searchValue = expression.Arguments[1].GetValue();
        string parameterValue = $"%{searchValue}%";

        if (expression.Arguments.Count == 3)
        {
            var operationArg = expression.Arguments[2].GetValue();
            string operation = operationArg?.ToString() ?? "starts";
            parameterValue = operation == "ends" ? $"$%{searchValue}" : $"{searchValue}%";
        }
        //Set column name.
        CreateColumn(expression.Arguments[0]);
        _sb.Append(" LIKE ");
        CreateParameter(parameterValue);
    }

    private void ResolveParameterized(MethodCallExpression expression)
    {
        //Resolve first arg string sql statement.
        ResolveRaw(expression);

        if (expression.Arguments.Count != 3) 
            return;
        //Using Conditions.ParameterizedSql("some string", [Property], [Field value]).
        //Assumes parameter name is the same as column name.
        var value = expression.Arguments[2].GetValue();
        var table = Tables.GetTable(expression.Arguments[1]);
        var column = new ColumnModel(expression.Arguments[1].ResolveName(), table.EntityType, table.TableAlias);
        var parameter = new ParameterModel<object?>(value, column.Name, column.SqlDataType);

        _parameterManager.AddParameters([parameter]);
    }

    #endregion Conditions

    #region Sql Functions

    private void ResolveAverage(MethodCallExpression expression)
    {
        _addRawValue = true;
        _sb.Append("AVG(");
        Visit(expression.Arguments[0]);  //use visitor in case arg has nested operator, i.e. column * n
        _sb.Append(')');
        _addRawValue = false;
    }

    private void ResolveCast(MethodCallExpression expression)
    {
        var constant = (ConstantExpression)expression.Arguments[1];

        _sb.Append("CAST(");
        Visit(expression.Arguments[0]);  //use visitor in case of nested functions.
        _sb.Append($" AS {constant.Value})");
    }

    private void ResolveCount(MethodCallExpression expression)
    {
        if (expression.Arguments.Count == 0)
        {
            _sb.Append("COUNT(*)");
            return;
        }

        _sb.Append("COUNT(");
        CreateColumn(expression.Arguments[0]);
        _sb.Append(')');
    }

    private void ResolveEoMonth(MethodCallExpression expression)
    {
        _sb.Append("EOMONTH(");
        Visit(expression.Arguments[0]);  //Use visitor since inputs could be property, field, value, or struct.
        _sb.Append(')');
    }

    private void ResolveDateDiff(MethodCallExpression expression)
    {
        var datePart = (ConstantExpression)expression.Arguments[0];

        _sb.Append($"DATEDIFF({datePart.Value}, ");
        //Use visitor since inputs could be property, field, or value.
        Visit(expression.Arguments[1]);
        _sb.Append(", ");
        Visit(expression.Arguments[2]);
        _sb.Append(')');
    }

    private void ResolveIif(MethodCallExpression expression)
    {
        _addRawValue = true;
        _sb.Append("IIF(");

        Visit(expression.Arguments[0]);
        _sb.Append(", ");
        Visit(expression.Arguments[1]);
        _sb.Append(", ");
        Visit(expression.Arguments[2]);
        _sb.Append(')');
        _addRawValue = false;
    }

    private void ResolveMinMax(MethodCallExpression expression)
    {
        string function = expression.Method.Name == "Min" ? "MIN" : "MAX";

        _addRawValue = true;
        _sb.Append($"{function}(");
        CreateColumn(expression.Arguments[0]);
        _sb.Append(')');
        _addRawValue = false;
    }

    private void ResolveRound(MethodCallExpression expression)
    {
        var constant = (ConstantExpression)expression.Arguments[1];

        _addRawValue = true;
        _sb.Append("ROUND(");

        if (MemberHelpers.ResolvesToMember(expression.Arguments[0], out MemberExpression? member))
            CreateColumn(member);
        else
            Visit(expression.Arguments[0]);  //Visit; has nested function.

        _sb.Append($", {constant.Value})");
        _addRawValue = false;
    }

    private void ResolveSum(MethodCallExpression expression)
    {
        _addRawValue = true;
        _sb.Append("SUM(");
        Visit(expression.Arguments[0]);
        _sb.Append(')');
        _addRawValue = false;
    }

    private void ResolveRaw(MethodCallExpression expression)
    {
        switch (expression.Arguments[0])
        {
            case ConstantExpression constant:
                _sb.Append(constant.Value);
                break;
            case MethodCallExpression method:  //Most likely an interpolation function.
                _sb.Append(method.GetValue());
                break;
            case MemberExpression member:  //Most likely field property.
                _sb.Append(member.GetValue());
                break;
        }
    }

    #endregion Sql Functions
}