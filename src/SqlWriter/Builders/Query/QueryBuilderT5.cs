using System.Linq.Expressions;
using SqlWriter.Interfaces;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Builders.Query;

public class QueryBuilderT5<T, T2, T3, T4, T5>(ITablesManager tables, IParameterManager parameterManager, string parameterPrefix = "p", string? unionStatement = null) 
    : BaseQueryBuilder(tables, parameterManager, parameterPrefix, unionStatement), IQuery<T, T2, T3, T4, T5> where T : class where T2 : class where T3 : class where T4 : class where T5 : class
{
    #region Select
    public IQuery<T, T2, T3, T4, T5> Select(Expression<Func<T, T2, T3, T4, T5, object>> columns)
    {
        SelectBase(columns);
        return this;
    }

    public IQuery<T, T2, T3, T4, T5> Select(params string[] columns)
    {
        SelectBase(columns);
        return this;
    }

    public IQuery<T, T2, T3, T4, T5> Select<TProjection>() where TProjection : class
    {
        SelectProjectionBase<TProjection>();
        return this;
    }

    public IQuery<T, T2, T3, T4, T5> SelectTop(int topValue)
    {
        SelectTopBase(topValue);
        return this;
    }

    public IQuery<T, T2, T3, T4, T5> SelectRaw(string statement, string aliasName)
    {
        SelectRawBase(statement, aliasName);
        return this;
    }

    public IQuery<T, T2, T3, T4, T5> SelectSubquery(string columnName, Func<ISubquery> subquery)
    {
        SelectSubqueryBase(columnName, subquery);
        return this;
    }

    #endregion Select

    #region Join
    public IQuery<T, T2, T3, T4, T5> Join<TTable1, TTable2>(JoinType joinType = JoinType.Inner) where TTable1 : class where TTable2 : class
    {
        JoinBase<TTable1, TTable2>(joinType);
        return this;
    }

    public IQuery<T, T2, T3, T4, T5> Join(Action<IJoinMapper> mapper)
    {
        JoinWithMapper(mapper);
        return this;
    }

    public IQuery<T, T2, T3, T4, T5> Join(Expression<Func<T, T2, T3, T4, T5, bool>> columns, JoinType joinType)
    {
        JoinBase(columns, joinType);
        return this;
    }

    #endregion Join

    #region With CTE
    public IQuery<T, T2, T3, T4, T5> With<TProperty>(Expression<Func<T, T2, T3, T4, T5, TProperty>> joinColumnName, ICteStatement cteStatement)
    {
        WithCte(joinColumnName, cteStatement);

        return this;
    }

    public IQuery<T, T2, T3, T4, T5> With<TCteJoinTable>(Expression<Func<T, T2, T3, T4, T5, TCteJoinTable, bool>> joinExpression, ICteStatement cteStatement) where TCteJoinTable : class
    {
        WithCte(joinExpression, cteStatement);

        return this;
    }

    public IQuery<T, T2, T3, T4, T5> WithLeft<TProperty>(Expression<Func<T, T2, T3, T4, T5, TProperty>> joinColumnName, ICteStatement cteStatement)
    {
        WithCte(joinColumnName, cteStatement, JoinType.Left);

        return this;
    }

    public IQuery<T, T2, T3, T4, T5> WithLeft<TCteJoinTable>(Expression<Func<T, T2, T3, T4, T5, TCteJoinTable, bool>> joinExpression, ICteStatement cteStatement) where TCteJoinTable : class
    {
        WithCte(joinExpression, cteStatement, JoinType.Left);

        return this;
    }

    #endregion With CTE

    #region  Where
    public IQuery<T, T2, T3, T4, T5> Where(Expression<Func<T, T2, T3, T4, T5, bool>> expression)
    {
        WhereBase(expression);
        return this;
    }

    public IQuery<T, T2, T3, T4, T5> WhereExists<TExists, TOuter>(Expression<Func<TExists, TOuter, bool>> expression) where TExists : class where TOuter : class
    {
        WhereExistsBase(expression);
        return this;
    }

    public IQuery<T, T2, T3, T4, T5> WhereNotExists<TExists, TOuter>(Expression<Func<TExists, TOuter, bool>> expression) where TExists : class where TOuter : class
    {
        WhereExistsBase(expression, true);
        return this;
    }

    public IQuery<T, T2, T3, T4, T5> WhereSubquery<TColumn>(Expression<Func<T, T2, T3, T4, T5, TColumn>> expression, Func<ISubquery> subquery)
    {
        WhereSubqueryBase(expression, subquery);
        return this;
    }

    public IQuery<T, T2, T3, T4, T5> WhereSubquery<TColumn>(Expression<Func<T, T2, T3, T4, T5, TColumn>> expression, ISubquery subquery)
    {
        WhereSubqueryBase(expression, subquery);
        return this;
    }

    #endregion Where

    #region Group by
    public IQuery<T, T2, T3, T4, T5> GroupBy<TProperty>(Expression<Func<T, T2, T3, T4, T5, TProperty>> column)
    {
        GroupByBase(column);
        return this;
    }

    public IQuery<T, T2, T3, T4, T5> GroupBy(Expression<Func<T, T2, T3, T4, T5, object>> column)
    {
        GroupByBase(column);
        return this;
    }

    public IQuery<T, T2, T3, T4, T5> Having(Expression<Func<T, T2, T3, T4, T5, bool>> expression)
    {
        HavingBase(expression);
        return this;
    }

    #endregion Group by

    #region Order by

    public IQuery<T, T2, T3, T4, T5> OrderBy(string column, string direction)
    {
        OrderByBase(column, direction);
        return this;
    }

    public IQuery<T, T2, T3, T4, T5> OrderByAsc<TProperty>(Expression<Func<T, T2, T3, T4, T5, TProperty>> column)
    {
        OrderByBase(column, "ASC");
        return this;
    }

    public IQuery<T, T2, T3, T4, T5> OrderByAsc(string column)
    {
        OrderByBase(column, "ASC");
        return this;
    }

    public IQuery<T, T2, T3, T4, T5> OrderByDesc<TProperty>(Expression<Func<T, T2, T3, T4, T5, TProperty>> column)
    {
        OrderByBase(column, "DESC");
        return this;
    }

    public IQuery<T, T2, T3, T4, T5> OrderByDesc(string column)
    {
        OrderByBase(column, "DESC");
        return this;
    }

    public IQuery<T, T2, T3, T4, T5> Pager(int pageIndex, int pageSize)
    {
        PagerBase(pageIndex, pageSize);
        return this;
    }

    #endregion Order by
}
