using System.Linq.Expressions;
using SqlWriter.Components.Where;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Builders.QueryCte;

public class CteBuilderT2<T, T2> : BaseCteBuilder, ICte<T, T2> where T : class where T2 : class
{
    public CteBuilderT2(ITablesManager tables, string cteAlias, bool stopColumnProjection, bool includeJoinColumn = false, string parameterPrefix = "p")
        : base(tables, cteAlias, stopColumnProjection, includeJoinColumn, parameterPrefix)
    {

    }

    #region Select
    public ICte<T, T2> Select(Expression<Func<T, object>> columns)
    {
        SelectBuilder.TranslateExpression(columns, ParameterPrefix);

        return this;
    }

    public ICte<T, T2> Select(params string[] columns)
    {
        foreach (string column in columns)
            SelectBuilder.AddColumn(column, column);

        return this;
    }

    public ICte<T, T2> Select<TProjection>() where TProjection : class
    {
        SelectBuilder.AddProjection<TProjection>();

        return this;
    }

    public ICte<T, T2> SelectRaw(string statement, string aliasName)
    {
        SelectBuilder.AddColumn(aliasName, $"{statement} AS [{aliasName}]");

        return this;
    }

    public ICte<T, T2> SelectTop(int topValue)
    {
        SelectBuilder.AddTopExpression(topValue);

        return this;
    }

    #endregion

    #region Join
    public ICte<T, T2> Join<TTable1, TTable2>(JoinType joinType = JoinType.Inner) where TTable1 : class where TTable2 : class
    {
        Tables.AddJoin<TTable1, TTable2>(joinType);

        return this;
    }

    public ICte<T, T2> Join(Expression<Func<T, T2, bool>> columns, JoinType joinType)
    {
        Tables.AddJoin(joinType, columns);

        return this;
    }

    public ICte<T, T2> JoinInner(Expression<Func<T, T2, bool>> columns)
    {
        Tables.AddJoin(JoinType.Inner, columns);

        return this;
    }

    public ICte<T, T2> JoinLeftOuter(Expression<Func<T, T2, bool>> columns)
    {
        Tables.AddJoin(JoinType.Left, columns);

        return this;
    }

    #endregion Join

    #region Where
    public ICte<T, T2> Where(Expression<Func<T, T2, bool>> expression)
    {
        WhereBuilder.TranslateExpression(expression, ParameterPrefix);

        return this;
    }

    public ICte<T, T2> WhereExists<TExists, TOuter>(Expression<Func<TExists, TOuter, bool>> expression) where TExists : class where TOuter : class
    {
        WhereExistsBuilder<TExists> existsBuilder = new(Translator, Tables);

        WhereBuilder.AddExist(existsBuilder.Compile(expression, ParameterPrefix));

        return this;
    }

    public ICte<T, T2> WhereNotExists<TExists, TOuter>(Expression<Func<TExists, TOuter, bool>> expression) where TExists : class where TOuter : class
    {
        WhereExistsBuilder<TExists> existsBuilder = new(Translator, Tables, true);

        WhereBuilder.AddExist(existsBuilder.Compile(expression, ParameterPrefix));

        return this;
    }

    public ICte<T, T2> WhereOr()
    {
        WhereBuilder.WhereOr();

        return this;
    }

    public ICte<T, T2> WhereSubquery<TColumn>(Expression<Func<T, TColumn>> expression, Func<ISubquery> subquery)
    {
        var compiled = subquery.Invoke();

        WhereBuilder.WhereSubquery(expression, compiled);

        return this;
    }

    public ICte<T, T2> WhereSubquery<TColumn>(Expression<Func<T, TColumn>> expression, ISubquery subquery)
    {
        WhereBuilder.WhereSubquery(expression, subquery);

        return this;
    }

    #endregion

    #region Group by
    public ICte<T, T2> GroupBy<TProperty>(Expression<Func<T, TProperty>> column)
    {
        GroupByBuilder.AddColumn(column);

        return this;
    }

    public ICte<T, T2> GroupBy(Expression<Func<T, object>> column)
    {
        GroupByBuilder.AddColumn(column);

        return this;
    }

    #endregion Group by

    #region Having
    public ICte<T, T2> Having(Expression<Func<T, bool>> expression)
    {
        HavingCondition = Translator.Translate(expression, doNotParameterizeValues: true);

        return this;
    }

    #endregion Having

    #region Order by
    public ICte<T, T2> OrderBy(string column, string direction)
    {
        OrderByBuilder.AddColumn(column, direction);

        return this;
    }

    public ICte<T, T2> OrderByAsc<TProperty>(Expression<Func<T, TProperty>> column)
    {
        OrderByBuilder.AddColumn(column, "ASC");

        return this;
    }

    public ICte<T, T2> OrderByAsc(string column)
    {
        OrderByBuilder.AddColumn(column, "ASC");

        return this;
    }

    public ICte<T, T2> OrderByDesc<TProperty>(Expression<Func<T, TProperty>> column)
    {
        OrderByBuilder.AddColumn(column, "DESC");

        return this;
    }

    public ICte<T, T2> OrderByDesc(string column)
    {
        OrderByBuilder.AddColumn(column, "DESC");

        return this;
    }

    #endregion Order by
}
