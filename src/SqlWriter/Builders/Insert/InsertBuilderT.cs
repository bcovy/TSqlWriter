using SqlWriter.Interfaces.Internals;
using System.Linq.Expressions;

namespace SqlWriter.Builders.Insert;

/// <summary>
/// Builds a SQL INSERT statement using a SELECT query as the data source.
/// </summary>
/// <typeparam name="TInsert">INSERT table entity.</typeparam>
/// <typeparam name="TSelect">SELECT table entity.</typeparam>
public class InsertBuilderT<TInsert, TSelect>(ITablesManager tables, IParameterManager parameterManager, string parameterPrefix = "p", string concatSqlStatement = "") 
    : BaseInsertBuilder(tables, parameterManager, typeof(TInsert), parameterPrefix, concatSqlStatement), IInsert<TInsert, TSelect> where TInsert : class where TSelect : class
{
    public IInsert<TInsert, TSelect> Into<TProjection>() where TProjection : class
    {
        InsertColumnsFromProjection<TProjection>();

        return this;
    }

    public IInsert<TInsert, TSelect> Into(Expression<Func<TInsert, object>> columns)
    {
        InsertColumnsFromExpression(columns);

        return this;
    }

    public IInsert<TInsert, TSelect> OutputTo<TOutputTo>(Expression<Func<TInsert, TOutputTo>> statement) where TOutputTo : class
    {
        OutputTo<TInsert, TOutputTo>(statement);

        return this;
    }

    public IInsert<TInsert, TSelect> Select<TProjection>() where TProjection : class
    {
        SelectBuilder.AddProjection<TProjection>();

        return this;
    }

    public IInsert<TInsert, TSelect> Select(Expression<Func<TSelect, object>> columns)
    {
        SelectBuilder.TranslateExpression(columns, ParameterPrefix);

        return this;
    }

    #region  Where
    public IInsert<TInsert, TSelect> Where(Expression<Func<TSelect, bool>> expression)
    {
        WhereBuilder.TranslateExpression(expression, ParameterPrefix);

        return this;
    }

    public IInsert<TInsert, TSelect> WhereExists<TExists>(Expression<Func<TExists, TSelect, bool>> expression) where TExists : class
    {
        AddWhereExists(expression);

        return this;
    }

    public IInsert<TInsert, TSelect> WhereNotExists<TExists>(Expression<Func<TExists, TSelect, bool>> expression) where TExists : class
    {
       AddWhereExists(expression, true);

        return this;
    }

    public IInsert<TInsert, TSelect> WhereSubquery<TColumn>(Expression<Func<TSelect, TColumn>> expression, Func<ISubquery> subquery)
    {
        WhereBuilder.WhereSubquery(expression, subquery);

        return this;
    }

    #endregion Where

    #region Group by
    public IInsert<TInsert, TSelect> GroupBy<TProperty>(Expression<Func<TSelect, TProperty>> column)
    {
        GroupByBuilder.AddColumn(column);

        return this;
    }

    public IInsert<TInsert, TSelect> GroupBy(Expression<Func<TSelect, object>> column)
    {
        GroupByBuilder.AddColumn(column);

        return this;
    }

    public IInsert<TInsert, TSelect> Having(Expression<Func<TSelect, bool>> condition)
    {
        HavingCondition = Translator.Translate(condition, doNotParameterizeValues: true);

        return this;
    }

    #endregion Group by
}