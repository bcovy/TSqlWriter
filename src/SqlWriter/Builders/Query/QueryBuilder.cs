using SqlWriter.Infrastructure;
using System.Linq.Expressions;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Builders.Query;

public class QueryBuilder(ITablesManager tables, IParameterManager parameterManager, bool dynamicWhere = false, string parameterPrefix = "p", string? unionStatement = null) 
    : BaseQueryBuilder(tables, parameterManager, parameterPrefix, unionStatement), IQueryBuilder
{
    private bool DynamicWhere { get; } = dynamicWhere;

    #region Select
    public IQueryBuilder Select(params string[] columns)
    {
        SelectBase(columns);
        return this;
    }

    #endregion Select

    #region Join
    
    private bool ContainsTableEntity<TTable>() where TTable : class
    {
        return Tables.Tables.ContainsKey(typeof(TTable));
    }
    
    public IQueryBuilder Join<TParentTable, TJoinTable>(Expression<Func<TParentTable, TJoinTable, bool>> columns, string joinTableAlias) 
        where TParentTable : class 
        where TJoinTable : class
    {
        AddJoin(columns, joinTableAlias, JoinType.Inner);
        return this;
    }

    public IQueryBuilder JoinLeft<TParentTable, TJoinTable>(Expression<Func<TParentTable, TJoinTable, bool>> columns, string joinTableAlias) 
        where TParentTable : class 
        where TJoinTable : class
    {
        AddJoin(columns, joinTableAlias, JoinType.Left);
        return this;
    }

    public IQueryBuilder JoinRight<TParentTable, TJoinTable>(Expression<Func<TParentTable, TJoinTable, bool>> columns, string joinTableAlias) 
        where TParentTable : class 
        where TJoinTable : class
    {
        AddJoin(columns, joinTableAlias, JoinType.Right);
        return this;
    }

    public IQueryBuilder With<TTable, TProperty>(Expression<Func<TTable, TProperty>> joinColumnName, ICteStatement cteStatement) where TTable : class
    {
        WithCte(joinColumnName, cteStatement);
        return this;
    }

    public IQueryBuilder With<TTable, TCteJoinTable>(Expression<Func<TTable, TCteJoinTable, bool>> joinExpression, ICteStatement cteStatement) where TTable : class where TCteJoinTable : class
    {
        WithCte(joinExpression, cteStatement);
        return this;
    }

    public IQueryBuilder WithLeft<TTable, TProperty>(Expression<Func<TTable, TProperty>> joinColumnName, ICteStatement cteStatement) where TTable : class
    {
        WithCte(joinColumnName, cteStatement, JoinType.Left);
        return this;
    }

    public IQueryBuilder WithLeft<TTable, TCteJoinTable>(Expression<Func<TTable, TCteJoinTable, bool>> joinExpression, ICteStatement cteStatement) where TTable : class where TCteJoinTable : class
    {
        WithCte(joinExpression, cteStatement, JoinType.Left);
        return this;
    }

    public IQueryBuilder WithRight<TTable, TProperty>(Expression<Func<TTable, TProperty>> joinColumnName, ICteStatement cteStatement) where TTable : class
    {
        WithCte(joinColumnName, cteStatement, JoinType.Right);
        return this;
    }

    private void AddJoin<TParentTable, TJoinTable>(Expression<Func<TParentTable, TJoinTable, bool>> columns, string joinTableAlias, JoinType joinType) 
        where TParentTable : class 
        where TJoinTable : class
    {
        if (!ContainsTableEntity<TParentTable>())
            throw new InvalidOperationException("Please add parent table entity before creating join.");

        Tables.AddTable<TJoinTable>(joinTableAlias);
        Tables.AddJoin(joinType, columns);
    }

    #endregion Join

    #region Where

    public IQueryBuilder WhereOr()
    {
        WhereBuilder.WhereOr();
        return this;
    }

    public IQueryBuilder WhereExists<TExists, TOuter>(Expression<Func<TExists, TOuter, bool>> expression) where TExists : class where TOuter : class
    {
        WhereExistsBase(expression);
        return this;
    }

    public IQueryBuilder WhereNotExists<TExists, TOuter>(Expression<Func<TExists, TOuter, bool>> expression) where TExists : class where TOuter : class
    {
        WhereExistsBase(expression, true);
        return this;
    }

    public IQueryBuilder Where<TTable>(Expression<Func<TTable, bool>> expression) where TTable : class
    {
        if (!ContainsTableEntity<TTable>())
            throw new InvalidOperationException("Missing associated entity table.");

        WhereBase(expression);
        return this;
    }

    public IQueryBuilder Where<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> expression) where TTable1 : class where TTable2 : class
    {
        if (!ContainsTableEntity<TTable1>())
            throw new InvalidOperationException("Missing associated entity table one.");

        if (!ContainsTableEntity<TTable2>())
            throw new InvalidOperationException("Missing associated entity table two.");

        WhereBase(expression);
        return this;
    }

    public IQueryBuilder Where<T>(string column, T value)
    {
        AddWhereColumnAndValue(column, value, Predicates.Equal);

        return this;
    }

    public IQueryBuilder WhereBetween<T>(string column, T value1, T value2)
    {
        if (TypeHelper.IsNumeric(value1))
            WhereBuilder.AddColumnAndValue(column, $"{value1} AND {value2}", Predicates.Between);
        else if (TypeHelper.IsDateTime(value1))
            WhereBuilder.AddColumnAndValue(column, $"'{value1:yyyy-MM-dd HH:mm:ss}' AND '{value2:yyyy-MM-dd HH:mm:ss}'", Predicates.Between);
        else
            WhereBuilder.AddColumnAndValue(column, $"'{value1}' AND '{value2}'", Predicates.Between);

        return this;
    }

    public IQueryBuilder WhereNotEqual<T>(string column, T value)
    {
        AddWhereColumnAndValue(column, value, Predicates.NotEqual);
        return this;
    }

    public IQueryBuilder WhereLessThan<T>(string column, T value)
    {
        AddWhereColumnAndValue(column, value, Predicates.LessThan);
        return this;
    }

    public IQueryBuilder WhereLessThanEqual<T>(string column, T value)
    {
        AddWhereColumnAndValue(column, value, Predicates.LessThanOrEqual);
        return this;
    }

    public IQueryBuilder WhereGreaterThan<T>(string column, T value)
    {
        AddWhereColumnAndValue(column, value, Predicates.GreaterThan);
        return this;
    }

    public IQueryBuilder WhereGreaterThanEqual<T>(string column, T value)
    {
        AddWhereColumnAndValue(column, value, Predicates.GreaterThanOrEqual);
        return this;
    }

    private void AddWhereColumnAndValue<T>(string column, T value, Predicates predicate)
    {
        if (DynamicWhere && (value is null || string.IsNullOrEmpty(value.ToString())))
            return;

        string parameterName = ParameterManager.Add(value, ParameterPrefix);

        WhereBuilder.AddColumnAndValue(column, parameterName, predicate);
    }

    public IQueryBuilder WhereLike(string column, string value)
    {
        if (DynamicWhere && string.IsNullOrEmpty(value))
            return this;
        
        string searchParameter = ParameterManager.Add($"%{value}%", ParameterPrefix);

        WhereBuilder.AddColumnAndValue(column, searchParameter, Predicates.Like);

        return this;
    }

    public IQueryBuilder WhereNotLike(string column, string value)
    {
        if (DynamicWhere && string.IsNullOrEmpty(value))
            return this;

        string searchParameter = ParameterManager.Add($"%{value}%", ParameterPrefix);

        WhereBuilder.AddColumnAndValue(column, searchParameter, Predicates.NotLike);

        return this;
    }

    public IQueryBuilder WhereSubquery(string column, Func<ISubquery> subquery)
    {
        var compiled = subquery.Invoke();
        WhereBuilder.WhereSubquery(column, compiled);
        return this;
    }

    public IQueryBuilder WhereSubquery(string column, ISubquery subquery)
    {
        WhereBuilder.WhereSubquery(column, subquery);
        return this;
    }

    #endregion Where

    #region Group By

    public IQueryBuilder GroupBy(string column)
    {
        GroupByBuilder.AddColumn(column);
        return this;
    }

    public IQueryBuilder Having<TTable>(Expression<Func<TTable, bool>> expression) where TTable : class
    {
        if (!ContainsTableEntity<TTable>())
            throw new InvalidOperationException("Please add Having table entity before creating statement.");

        HavingBase(expression);
        return this;
    }
    #endregion Group By

    #region Order By
    public IQueryBuilder OrderByAsc(string column)
    {
        OrderByBase(column, "ASC");
        return this;
    }

    public IQueryBuilder OrderByDesc(string column)
    {
        OrderByBase(column, "DESC");
        return this;
    }

    public IQueryBuilder OrderBy(string column, string direction)
    {
        OrderByBase(column, direction);
        return this;
    }

    public IQueryBuilder Pager(int pageIndex, int pageSize)
    {
        PagerBase(pageIndex, pageSize);
        return this;
    }

    #endregion Order By
}