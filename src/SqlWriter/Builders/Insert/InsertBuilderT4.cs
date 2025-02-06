using System.Linq.Expressions;
using SqlWriter.Interfaces;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Builders.Insert;

/// <summary>
/// Builds a SQL INSERT statement using a SELECT query as the data source.
/// </summary>
/// <typeparam name="TInsert">INSERT table entity.</typeparam>
/// <typeparam name="T">SELECT table entity.</typeparam>
/// <typeparam name="T2">SELECT table entity.</typeparam>
/// <typeparam name="T3">SELECT table entity.</typeparam>
/// <typeparam name="T4">SELECT table entity.</typeparam>
public class InsertBuilderT4<TInsert, T, T2, T3, T4>(ITablesManager tables, IParameterManager parameterManager, string parameterPrefix = "p", string concatSqlStatement = "") 
    : BaseInsertBuilder(tables, parameterManager, typeof(TInsert), parameterPrefix, concatSqlStatement),
    IInsert<TInsert, T, T2, T3, T4> 
    where TInsert : class 
    where T : class 
    where T2 : class 
    where T3 : class 
    where T4 : class
{

    public IInsert<TInsert, T, T2, T3, T4> Into(Expression<Func<TInsert, object>> columns)
    {
        InsertColumnsFromExpression(columns);

        return this;
    }

    public IInsert<TInsert, T, T2, T3, T4> OutputTo<TOutputTo>(Expression<Func<TInsert, TOutputTo>> statement) where TOutputTo : class
    {
        OutputTo<TInsert, TOutputTo>(statement);

        return this;
    }

    public IInsert<TInsert, T, T2, T3, T4> Select(Expression<Func<T, T2, T3, T4, object>> columns)
    {
        SelectBuilder.TranslateExpression(columns, ParameterPrefix);

        return this;
    }

    #region Join
    public IInsert<TInsert, T, T2, T3, T4> Join<TTable1, TTable2>(JoinType joinType = JoinType.Inner) where TTable1 : class where TTable2 : class
    {
        Tables.AddJoin<TTable1, TTable2>(joinType);

        return this;
    }

    public IInsert<TInsert, T, T2, T3, T4> Join(Action<IJoinMapper> mapper)
    {
        Tables.JoinWithMapper(mapper);

        return this;
    }

    public IInsert<TInsert, T, T2, T3, T4> Join(Expression<Func<T, T2, T3, T4, bool>> columns, JoinType joinType)
    {
        Tables.AddJoin(joinType, columns);

        return this;
    }

    public IInsert<TInsert, T, T2, T3, T4> JoinInner(Expression<Func<T, T2, T3, T4, bool>> columns)
    {
        Tables.AddJoin(JoinType.Inner, columns);

        return this;
    }

    public IInsert<TInsert, T, T2, T3, T4> JoinLeftOuter(Expression<Func<T, T2, T3, T4, bool>> columns)
    {
        Tables.AddJoin(JoinType.Left, columns);

        return this;
    }

    #endregion Join

    #region  Where
    public IInsert<TInsert, T, T2, T3, T4> Where(Expression<Func<T, T2, T3, T4, bool>> expression)
    {
        WhereBuilder.TranslateExpression(expression, ParameterPrefix);

        return this;
    }

    public IInsert<TInsert, T, T2, T3, T4> WhereExists<TExists, TOuter>(Expression<Func<TExists, TOuter, bool>> expression) where TExists : class where TOuter : class
    {
        AddWhereExists(expression);

        return this;
    }

    public IInsert<TInsert, T, T2, T3, T4> WhereNotExists<TExists, TOuter>(Expression<Func<TExists, TOuter, bool>> expression) where TExists : class where TOuter : class
    {
        AddWhereExists(expression, true);

        return this;
    }

    public IInsert<TInsert, T, T2, T3, T4> WhereSubquery<TColumn>(Expression<Func<T, T2, T3, T4, TColumn>> expression, Func<ISubquery> subquery)
    {
        WhereBuilder.WhereSubquery(expression, subquery);

        return this;
    }

    #endregion Where

    #region Group by
    public IInsert<TInsert, T, T2, T3, T4> GroupBy<TProperty>(Expression<Func<T, T2, T3, T4, TProperty>> column)
    {
        GroupByBuilder.AddColumn(column);

        return this;
    }

    public IInsert<TInsert, T, T2, T3, T4> GroupBy(Expression<Func<T, T2, T3, T4, object>> column)
    {
        GroupByBuilder.AddColumn(column);

        return this;
    }

    public IInsert<TInsert, T, T2, T3, T4> Having(Expression<Func<T, T2, T3, T4, bool>> condition)
    {
        HavingCondition = Translator.Translate(condition, doNotParameterizeValues: true);

        return this;
    }

    #endregion Group by
}