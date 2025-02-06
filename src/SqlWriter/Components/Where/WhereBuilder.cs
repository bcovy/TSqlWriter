using System.Linq.Expressions;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Components.Where;

public class WhereBuilder(IExpressionSqlTranslator translator, IParameterManager parameterManager)
{
    private string _nextPrefix = "AND";

    public bool HasConditions => Conditions.Count > 0;
    public List<(string, string)> Conditions { get; } = [];

    /// <summary>
    /// Returns compiled WHERE clause.  Result will include empty space before the word 'WHERE'.
    /// </summary>
    /// <returns>Compiled WHERE clause.</returns>
    public string Compile()
    {
        if (Conditions.Count == 1)
            return $" WHERE {Conditions[0].Item2}";

        string second = string.Concat(Conditions[1..].Select(x => $" {x.Item1} {x.Item2}"));

        return $" WHERE {Conditions[0].Item2}{second}";
    }

    public void WhereOr()
    {
        _nextPrefix = "OR";
    }

    public void AddExist(string statement)
    {
        Conditions.Insert(0, ("", statement));
    }

    public void AddColumnAndValue<T>(string column, T value, Predicates predicate)
    {
        string condition = TranslatePredicate(predicate);

        Conditions.Add((_nextPrefix, $"{column} {condition} {value}"));
        _nextPrefix = "AND";  //Set prefix back to default.
    }

    public void TranslateExpression(LambdaExpression expression, string alias = "pw", bool excludeTableAlias = false)
    {
        string statement = excludeTableAlias ? translator.TranslateWithoutAlias(expression, alias) : translator.Translate(expression, alias);

        Conditions.Add((_nextPrefix, $"{statement}"));
        _nextPrefix = "AND";  //Set prefix back to default.
    }

    public void WhereSubquery(LambdaExpression expression, Func<ISubquery> subquery, bool excludeTableAlias = false)
    {
        var compiled = subquery.Invoke();

        WhereSubquery(expression, compiled, excludeTableAlias);
    }

    public void WhereSubquery(LambdaExpression expression, ISubquery subquery, bool excludeTableAlias = false)
    {
        string column = excludeTableAlias ? translator.TranslateWithoutAlias(expression) : translator.Translate(expression);

        WhereSubquery(column, subquery);
    }

    public void WhereSubquery(string column, ISubquery subquery)
    {
        string condition = TranslatePredicate(subquery.ConditionPredicate);

        Conditions.Add(($"{subquery.ConditionPrefix}", $"{column} {condition} {subquery.GetSqlStatement()}"));
        //Add subquery parameters to current collection.
        parameterManager.AddParameters(subquery.Parameters);
    }

    private static string TranslatePredicate(Predicates predicate) => predicate switch
    {
        Predicates.Equal => "=",
        Predicates.NotEqual => "<>",
        Predicates.LessThan => "<",
        Predicates.LessThanOrEqual => "<=",
        Predicates.GreaterThan => ">",
        Predicates.GreaterThanOrEqual => ">=",
        Predicates.Like => "LIKE",
        Predicates.NotLike => "NOT LIKE",
        Predicates.Between => "BETWEEN",
        _ => throw new NotImplementedException()
    };
}