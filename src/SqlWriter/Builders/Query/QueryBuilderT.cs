using SqlWriter.Interfaces.Internals;
using System.Linq.Expressions;

namespace SqlWriter.Builders.Query;

/// <summary>
/// Builds a SQL query statement for a single table.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public class QueryBuilderT<T>(ITablesManager tables, IParameterManager parameterManager, string parameterPrefix = "p", string? unionStatement = null) 
    : BaseQueryBuilder(tables, parameterManager, parameterPrefix, unionStatement), IQuery<T> where T : class
{
    #region Select

    public IQuery<T> SelectAll()
    {
        SelectAllBase();
        return this;
    }

    public IQuery<T> Select(Expression<Func<T, object>> columns)
    {
        SelectBase(columns);
        return this;
    }

    public IQuery<T> Select(params string[] columns)
    {
        SelectBase(columns);
        return this;
    }

    public IQuery<T> Select<TProjection>() where TProjection : class
    {
        SelectProjectionBase<TProjection>();
        return this;
    }

    public IQuery<T> SelectTop(int topValue)
    {
        SelectTopBase(topValue);
        return this;
    }

    public IQuery<T> SelectCount(string aliasName)
    {
        SelectRawBase("COUNT(*)", aliasName);
        return this;
    }

    public IQuery<T> SelectRaw(string statement, string aliasName)
    {
        SelectRawBase(statement, aliasName);
        return this;
    }

    public IQuery<T> SelectSubquery(string columnName, ISubquery subquery)
    {
        SelectSubqueryBase(columnName, subquery);
        return this;
    }

    public IQuery<T> SelectSubquery(string columnName, Func<ISubquery> subquery)
    {
        SelectSubqueryBase(columnName, subquery);
        return this;
    }

    #endregion Select

    #region With CTE

    public IQuery<T> With<TProperty>(Expression<Func<T, TProperty>> joinColumnName, ICteStatement cteStatement)
    {
        WithCte(joinColumnName, cteStatement);

        return this;
    }

    public IQuery<T> With<TCteJoinTable>(Expression<Func<T, TCteJoinTable, bool>> joinExpression, ICteStatement cteStatement) where TCteJoinTable : class
    {
        WithCte(joinExpression, cteStatement);

        return this;
    }

    public IQuery<T> WithLeft<TProperty>(Expression<Func<T, TProperty>> joinColumnName, ICteStatement cteStatement)
    {
        WithCte(joinColumnName, cteStatement, JoinType.Left);

        return this;
    }

    public IQuery<T> WithLeft<TCteJoinTable>(Expression<Func<T, TCteJoinTable, bool>> joinExpression, ICteStatement cteStatement) where TCteJoinTable : class
    {
        WithCte(joinExpression, cteStatement, JoinType.Left);

        return this;
    }

    #endregion With CTE

    #region  Where
    public IQuery<T> Where(Expression<Func<T, bool>> expression)
    {
        WhereBase(expression);
        return this;
    }

    public IQuery<T> WhereExists<TExists>(Expression<Func<TExists, T, bool>> expression) where TExists : class
    {
        WhereExistsBase(expression);
        return this;
    }

    public IQuery<T> WhereNotExists<TExists>(Expression<Func<TExists, T, bool>> expression) where TExists : class
    {
        WhereExistsBase(expression, true);
        return this;
    }

    public IQuery<T> WhereSubquery<TColumn>(Expression<Func<T, TColumn>> expression, Func<ISubquery> subquery)
    {
        WhereSubqueryBase(expression, subquery);
        return this;
    }

    public IQuery<T> WhereSubquery<TColumn>(Expression<Func<T, TColumn>> expression, ISubquery subquery)
    {
        WhereSubqueryBase(expression, subquery);
        return this;
    }

    #endregion Where

    #region Group by
    public IQuery<T> GroupBy<TProperty>(Expression<Func<T, TProperty>> column)
    {
        GroupByBase(column);
        return this;
    }

    public IQuery<T> GroupBy(Expression<Func<T, object>> column)
    {
        GroupByBase(column);
        return this;
    }

    #endregion Group by

    #region Having
    public IQuery<T> Having(Expression<Func<T, bool>> expression)
    {
        HavingBase(expression);
        return this;
    }

    #endregion Having

    #region Order by
    public IQuery<T> OrderBy(string column, string direction)
    {
        OrderByBase(column, direction);
        return this;
    }

    public IQuery<T> OrderByAsc<TProperty>(Expression<Func<T, TProperty>> column)
    {
        OrderByBase(column, "ASC");
        return this;
    }

    public IQuery<T> OrderByAsc(string column)
    {
        OrderByBase(column, "ASC");
        return this;
    }

    public IQuery<T> OrderByDesc<TProperty>(Expression<Func<T, TProperty>> column)
    {
        OrderByBase(column, "DESC");
        return this;
    }

    public IQuery<T> OrderByDesc(string column)
    {
        OrderByBase(column, "DESC");
        return this;
    }

    public IQuery<T> Pager(int pageIndex, int pageSize)
    {
        PagerBase(pageIndex, pageSize);
        return this;
    }

    #endregion Order by
}