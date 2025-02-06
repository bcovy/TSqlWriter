using System.Linq.Expressions;
using SqlWriter.Components.Where;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Builders.QueryCte;

public class CteBuilderT<T> : BaseCteBuilder, ICte<T> where T : class
{
    public CteBuilderT(ITablesManager tables, string cteAlias, bool stopColumnProjection, bool includeJoinColumn = false, string parameterPrefix = "p")
        : base(tables, cteAlias, stopColumnProjection, includeJoinColumn, parameterPrefix)
    {

    }

    #region Select
    public ICte<T> Select(Expression<Func<T, object>> columns)
    {
        SelectBuilder.TranslateExpression(columns, ParameterPrefix);

        return this;
    }

    public ICte<T> Select(params string[] columns)
    {
        foreach (string column in columns)
            SelectBuilder.AddColumn(column, column);

        return this;
    }

    public ICte<T> Select<TProjection>() where TProjection : class
    {
        SelectBuilder.AddProjection<TProjection>();

        return this;
    }

    public ICte<T> SelectRaw(string statement, string aliasName)
    {
        SelectBuilder.AddColumn(aliasName, $"{statement} AS [{aliasName}]");

        return this;
    }

    public ICte<T> SelectTop(int topValue)
    {
        SelectBuilder.AddTopExpression(topValue);

        return this;
    }

    #endregion

    #region Where
    public ICte<T> Where(Expression<Func<T, bool>> expression)
    {
        WhereBuilder.TranslateExpression(expression, ParameterPrefix);

        return this;
    }

    public ICte<T> WhereOr()
    {
        WhereBuilder.WhereOr();

        return this;
    }

    public ICte<T> WhereExists<TExists>(Expression<Func<TExists, T, bool>> expression) where TExists : class
    {
        WhereExistsBuilder<TExists> existsBuilder = new(Translator, Tables);

        WhereBuilder.AddExist(existsBuilder.Compile(expression, ParameterPrefix));

        return this;
    }

    public ICte<T> WhereNotExists<TExists>(Expression<Func<TExists, T, bool>> expression) where TExists : class
    {
        WhereExistsBuilder<TExists> existsBuilder = new(Translator, Tables, true);

        WhereBuilder.AddExist(existsBuilder.Compile(expression, ParameterPrefix));

        return this;
    }

    public ICte<T> WhereSubquery<TColumn>(Expression<Func<T, TColumn>> expression, Func<ISubquery> subquery)
    {
        var compiled = subquery.Invoke();

        WhereBuilder.WhereSubquery(expression, compiled);

        return this;
    }

    public ICte<T> WhereSubquery<TColumn>(Expression<Func<T, TColumn>> expression, ISubquery subquery)
    {
        WhereBuilder.WhereSubquery(expression, subquery);

        return this;
    }

    #endregion

    #region Group by
    public ICte<T> GroupBy<TProperty>(Expression<Func<T, TProperty>> column)
    {
        GroupByBuilder.AddColumn(column);

        return this;
    }

    public ICte<T> GroupBy(Expression<Func<T, object>> column)
    {
        GroupByBuilder.AddColumn(column);

        return this;
    }

    #endregion Group by

    #region Having
    public ICte<T> Having(Expression<Func<T, bool>> expression)
    {
        HavingCondition = Translator.Translate(expression, doNotParameterizeValues: true);

        return this;
    }

    #endregion Having

    #region Order by
    public ICte<T> OrderBy(string column, string direction)
    {
        OrderByBuilder.AddColumn(column, direction);

        return this;
    }

    public ICte<T> OrderByAsc<TProperty>(Expression<Func<T, TProperty>> column)
    {
        OrderByBuilder.AddColumn(column, "ASC");

        return this;
    }

    public ICte<T> OrderByAsc(string column)
    {
        OrderByBuilder.AddColumn(column, "ASC");

        return this;
    }

    public ICte<T> OrderByDesc<TProperty>(Expression<Func<T, TProperty>> column)
    {
        OrderByBuilder.AddColumn(column, "DESC");

        return this;
    }

    public ICte<T> OrderByDesc(string column)
    {
        OrderByBuilder.AddColumn(column, "DESC");

        return this;
    }

    #endregion Order by
}
