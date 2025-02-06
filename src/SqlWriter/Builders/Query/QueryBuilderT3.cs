using System.Linq.Expressions;
using SqlWriter.Interfaces;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Builders.Query;

/// <summary>
/// Builds a SQL query statement from 3 tables.
/// </summary>
/// <typeparam name="T">Entity table type.</typeparam>
/// <typeparam name="T2">Entity table type.</typeparam>
/// <typeparam name="T3">Entity table type.</typeparam>
public class QueryBuilderT3<T, T2, T3>(ITablesManager tables, IParameterManager parameterManager, string parameterPrefix = "p", string? unionStatement = null) 
    : BaseQueryBuilder(tables, parameterManager, parameterPrefix, unionStatement), IQuery<T, T2, T3> where T : class where T2 : class where T3 : class
{
    #region Select

    public IQuery<T, T2, T3> SelectAll()
    {
        SelectAllBase();
        return this;
    }

    public IQuery<T, T2, T3> Select(Expression<Func<T, T2, T3, object>> columns)
    {
        SelectBase(columns);
        return this;
    }

    public IQuery<T, T2, T3> Select(params string[] columns)
    {
        SelectBase(columns);
        return this;
    }

    public IQuery<T, T2, T3> Select<TProjection>() where TProjection : class
    {
        SelectProjectionBase<TProjection>();
        return this;
    }

    public IQuery<T, T2, T3> SelectTop(int topValue)
    {
        SelectTopBase(topValue);
        return this;
    }

    public IQuery<T, T2, T3> SelectRaw(string statement, string aliasName)
    {
        SelectRawBase(statement, aliasName);
        return this;
    }

    public IQuery<T, T2, T3> SelectSubquery(string columnName, Func<ISubquery> subquery)
    {
        SelectSubqueryBase(columnName, subquery);
        return this;
    }

    #endregion Select

    #region Join
    public IQuery<T, T2, T3> Join<TTable1, TTable2>(JoinType joinType = JoinType.Inner) where TTable1 : class where TTable2 : class
    {
        JoinBase<TTable1, TTable2>(joinType);
        return this;
    }

    public IQuery<T, T2, T3> Join(Action<IJoinMapper> mapper)
    {
        JoinWithMapper(mapper);
        return this;
    }

    public IQuery<T, T2, T3> Join(Expression<Func<T, T2, T3, bool>> columns, JoinType joinType)
    {
        JoinBase(columns, joinType);
        return this;
    }

    public IQuery<T, T2, T3> JoinInner(Expression<Func<T, T2, T3, bool>> columns)
    {
        JoinBase(columns, JoinType.Inner);
        return this;
    }

    public IQuery<T, T2, T3> JoinLeftOuter(Expression<Func<T, T2, T3, bool>> columns)
    {
        JoinBase(columns, JoinType.Left);
        return this;
    }

    #endregion Join

    #region With CTE
    public IQuery<T, T2, T3> With<TProperty>(Expression<Func<T, T2, T3, TProperty>> joinColumnName, ICteStatement cteStatement)
    {
        WithCte(joinColumnName, cteStatement);

        return this;
    }

    public IQuery<T, T2, T3> With<TCteJoinTable>(Expression<Func<T, T2, T3, TCteJoinTable, bool>> joinExpression, ICteStatement cteStatement) where TCteJoinTable : class
    {
        WithCte(joinExpression, cteStatement);

        return this;
    }

    public IQuery<T, T2, T3> WithLeft<TProperty>(Expression<Func<T, T2, T3, TProperty>> joinColumnName, ICteStatement cteStatement)
    {
        WithCte(joinColumnName, cteStatement, JoinType.Left);

        return this;
    }

    public IQuery<T, T2, T3> WithLeft<TCteJoinTable>(Expression<Func<T, T2, T3, TCteJoinTable, bool>> joinExpression, ICteStatement cteStatement) where TCteJoinTable : class
    {
        WithCte(joinExpression, cteStatement, JoinType.Left);

        return this;
    }

    #endregion With CTE

    #region  Where
    public IQuery<T, T2, T3> Where(Expression<Func<T, T2, T3, bool>> expression)
    {
        WhereBase(expression);
        return this;
    }

    public IQuery<T, T2, T3> WhereExists<TExists, TOuter>(Expression<Func<TExists, TOuter, bool>> expression) where TExists : class where TOuter : class
    {
        WhereExistsBase(expression);
        return this;
    }

    public IQuery<T, T2, T3> WhereNotExists<TExists, TOuter>(Expression<Func<TExists, TOuter, bool>> expression) where TExists : class where TOuter : class
    {
        WhereExistsBase(expression, true);
        return this;
    }

    public IQuery<T, T2, T3> WhereSubquery<TColumn>(Expression<Func<T, T2, T3, TColumn>> expression, Func<ISubquery> subquery)
    {
        WhereSubqueryBase(expression, subquery);
        return this;
    }

    public IQuery<T, T2, T3> WhereSubquery<TColumn>(Expression<Func<T, T2, T3, TColumn>> expression, ISubquery subquery)
    {
        WhereSubqueryBase(expression, subquery);
        return this;
    }

    #endregion Where

    #region Group by
    public IQuery<T, T2, T3> GroupBy<TProperty>(Expression<Func<T, T2, T3, TProperty>> column)
    {
        GroupByBase(column);
        return this;
    }

    public IQuery<T, T2, T3> GroupBy(Expression<Func<T, T2, T3, object>> column)
    {
        GroupByBase(column);
        return this;
    }

    public IQuery<T, T2, T3> Having(Expression<Func<T, T2, T3, bool>> expression)
    {
        HavingBase(expression);
        return this;
    }

    #endregion Group by

    #region Order by

    public IQuery<T, T2, T3> OrderBy(string column, string direction)
    {
        OrderByBase(column, direction);
        return this;
    }

    public IQuery<T, T2, T3> OrderByAsc<TProperty>(Expression<Func<T, T2, T3, TProperty>> column)
    {
        OrderByBase(column, "ASC");
        return this;
    }

    public IQuery<T, T2, T3> OrderByAsc(string column)
    {
        OrderByBase(column, "ASC");
        return this;
    }

    public IQuery<T, T2, T3> OrderByDesc<TProperty>(Expression<Func<T, T2, T3, TProperty>> column)
    {
        OrderByBase(column, "DESC");
        return this;
    }

    public IQuery<T, T2, T3> OrderByDesc(string column)
    {
        OrderByBase(column, "DESC");
        return this;
    }

    public IQuery<T, T2, T3> Pager(int pageIndex, int pageSize)
    {
        PagerBase(pageIndex, pageSize);
        return this;
    }

    #endregion Order by
}