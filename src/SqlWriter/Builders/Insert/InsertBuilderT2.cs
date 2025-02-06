using System.Linq.Expressions;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Builders.Insert;

/// <summary>
/// Builds a SQL INSERT statement using a SELECT query as the data source.
/// </summary>
/// <typeparam name="TInsert">INSERT table entity.</typeparam>
/// <typeparam name="T">SELECT table entity.</typeparam>
/// <typeparam name="T2">SELECT table entity.</typeparam>
public class InsertBuilderT2<TInsert, T, T2>(ITablesManager tables, IParameterManager parameterManager, string parameterPrefix = "p", string concatSqlStatement = "") 
    : BaseInsertBuilder(tables, parameterManager, typeof(TInsert), parameterPrefix, concatSqlStatement),
    IInsert<TInsert, T, T2> where TInsert : class where T : class where T2 : class
{ 
    public IInsert<TInsert, T, T2> Into<TProjection>() where TProjection : class
    {
        InsertColumnsFromProjection<TProjection>();

        return this;
    }

    public IInsert<TInsert, T, T2> Into(Expression<Func<TInsert, object>> columns)
    {
        InsertColumnsFromExpression(columns);

        return this;
    }

    public IInsert<TInsert, T, T2> OutputTo<TOutputTo>(Expression<Func<TInsert, TOutputTo>> statement) where TOutputTo : class
    {
        OutputTo<TInsert, TOutputTo>(statement);

        return this;
    }

    public IInsert<TInsert, T, T2> Select<TProjection>() where TProjection : class
    {
        SelectBuilder.AddProjection<TProjection>();

        return this;
    }

    public IInsert<TInsert, T, T2> Select(Expression<Func<T, T2, object>> columns)
    {
        SelectBuilder.TranslateExpression(columns, ParameterPrefix);

        return this;
    }

    #region Join
    public IInsert<TInsert, T, T2> Join<TTable1, TTable2>(JoinType joinType = JoinType.Inner) where TTable1 : class where TTable2 : class
    {
        Tables.AddJoin<TTable1, TTable2>(joinType);

        return this;
    }

    public IInsert<TInsert, T, T2> Join(Expression<Func<T, T2, bool>> columns, JoinType joinType = JoinType.Inner)
    {
        Tables.AddJoin(joinType, columns);

        return this;
    }

    #endregion Join

    #region  Where
    public IInsert<TInsert, T, T2> Where(Expression<Func<T, T2, bool>> expression)
    {
        WhereBuilder.TranslateExpression(expression, ParameterPrefix);

        return this;
    }

    public IInsert<TInsert, T, T2> WhereExists<TExists, TOuter>(Expression<Func<TExists, TOuter, bool>> expression) where TExists : class where TOuter : class
    {
        AddWhereExists(expression);

        return this;
    }

    public IInsert<TInsert, T, T2> WhereNotExists<TExists, TOuter>(Expression<Func<TExists, TOuter, bool>> expression) where TExists : class where TOuter : class
    {
        AddWhereExists(expression, true);

        return this;
    }

    public IInsert<TInsert, T, T2> WhereSubquery<TColumn>(Expression<Func<T, T2, TColumn>> expression, Func<ISubquery> subquery)
    {
        WhereBuilder.WhereSubquery(expression, subquery);

        return this;
    }

    #endregion Where

    #region Group by
    public IInsert<TInsert, T, T2> GroupBy<TProperty>(Expression<Func<T, T2, TProperty>> column)
    {
        GroupByBuilder.AddColumn(column);

        return this;
    }

    public IInsert<TInsert, T, T2> GroupBy(Expression<Func<T, T2, object>> column)
    {
        GroupByBuilder.AddColumn(column);

        return this;
    }

    public IInsert<TInsert, T, T2> Having(Expression<Func<T, T2, bool>> condition)
    {
        HavingCondition = Translator.Translate(condition, doNotParameterizeValues: true);

        return this;
    }

    #endregion Group by
}