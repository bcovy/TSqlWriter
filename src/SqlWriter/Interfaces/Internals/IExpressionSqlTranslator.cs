using System.Linq.Expressions;
using SqlWriter.Components.Tables;

namespace SqlWriter.Interfaces.Internals;

public interface IExpressionSqlTranslator
{
    Stack<ColumnModel> Columns { get; }
    string Translate(Expression expression, string parameterNamePrefix = "p", bool doNotParameterizeValues = false);
    string TranslateWithoutAlias(Expression expression, string parameterNamePrefix = "p", bool doNotParameterizeValues = false);
}