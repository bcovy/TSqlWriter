using System.Linq.Expressions;

namespace SqlWriter;

/// <summary>
/// Builds a SQL UPDATE statement using a SELECT query as the data source.  Inherits <see cref="ISqlStatement"/>.
/// </summary>
/// <typeparam name="TUpdate">UPDATE table entity.</typeparam>
/// <typeparam name="TSelect">SELECT table entity.</typeparam>
public interface IUpdate<TUpdate, TSelect> : ISqlStatement where TUpdate : class where TSelect : class
{
    /// <summary>
    /// Concatenates the results of the current statement into the next statement to follow.
    /// </summary>
    /// <remarks>
    /// It is recommended that building of the next statement be done using the fluent build methods on this object.  This helps to 
    /// ensure the last statement in the chain is able to compile all preceding statements into one. If you choose to break up 
    /// the fluent build process, be aware that the value type will be different that the one used to hold the instantiated value.
    /// </remarks>
    /// <returns><see cref="IConcatSql"/> object to allow user to concatenate one or more SQL statements.</returns>
    IConcatSql Concat();
    /// <summary>
    /// Specify column to update using value type of <typeparamref name="TProperty"/>.
    /// </summary>
    /// <typeparam name="TProperty">Column type.</typeparam>
    /// <param name="column">Target column expression.</param>
    /// <param name="value">Update value.</param>
    /// <returns><see cref="IUpdate{TUpdate, TSelect}"/> object to allow user to chain statement methods.</returns>
    IUpdate<TUpdate, TSelect> Set<TProperty>(Expression<Func<TUpdate, TProperty>> column, TProperty value);
    /// <summary>
    /// Specify column to update using a value derived from <typeparamref name="TSelect"/> entity column.
    /// </summary>
    /// <remarks>
    /// Use this method if you want value to include a <see cref="SqlFunc"/> result, or if <typeparamref name="T"/> and <typeparamref name="T2"/> 
    /// are of the same value type, but one is <see langword="Nullable"/>.
    /// </remarks>
    /// <typeparam name="T">Column type.</typeparam>
    /// <typeparam name="T2">Value type.</typeparam>
    /// <param name="column">Target column.</param>
    /// <param name="statement">Source column from SELECT entity.</param>
    /// <returns><see cref="IUpdate{TUpdate, TSelect}"/> object to allow user to chain statement methods.</returns>
    IUpdate<TUpdate, TSelect> Set<T, T2>(Expression<Func<TUpdate, T>> column, Expression<Func<TSelect, T2>> statement);
    /// <summary>
    /// Specify column to update to <see langword="null"/>.
    /// </summary>
    /// <param name="column">Target column expression.</param>
    /// <typeparam name="TProperty">Column type.</typeparam>
    /// <returns><see cref="IUpdate{TUpdate, TSelect}"/> object to allow user to chain statement methods.</returns>
    IUpdate<TUpdate, TSelect> SetNull<TProperty>(Expression<Func<TUpdate, TProperty>> column);
    /// <summary>
    /// Specify column to update using an un-parameterized string value.
    /// </summary>
    /// <param name="columnName">Column name.</param>
    /// <param name="columnValue">Update value.</param>
    /// <returns><see cref="IUpdate{TUpdate, TSelect}"/> object to allow user to chain statement methods.</returns>
    IUpdate<TUpdate, TSelect> SetRaw(string columnName, string columnValue);
    /// <summary>
    /// Specify column to update using an un-parameterized string value.
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="expression">Target column expression.</param>
    /// <param name="columnValue">Update value.</param>
    /// <returns><see cref="IUpdate{TUpdate, TSelect}"/> object to allow user to chain statement methods.</returns>
    IUpdate<TUpdate, TSelect> SetRaw<TProperty>(Expression<Func<TUpdate, TProperty>> expression, string columnValue);
    /// <summary>
    /// Adds an OUTPUT clause to return information from each row affected by the UPDATE statement.
    /// <example><code>(o) => new UpdateOutput[Entity]() { Inserted = new Entity { EventID = o.EventID, Comments = o.Grade } }</code></example>
    /// </summary>
    /// <typeparam name="TOutputTo">OUTPUT INTO entity table where results will be saved.</typeparam>
    /// <param name="statement"><see cref="MemberInitExpression"/> expression that maps inserted columns to output table.</param>
    /// <returns><see cref="IUpdate{TUpdate, TSelect}"/> object to allow user to chain statement methods.</returns>
    public IUpdate<TUpdate, TSelect> OutputTo<TOutputTo>(Expression<Func<TUpdate, UpdateOutput<TOutputTo>>> statement) where TOutputTo : class;
    /// <summary>
    /// Specifies the filter condition for the target table(s).
    /// </summary>
    /// <param name="expression">Search condition expression.</param>
    /// <returns><see cref="IUpdate{TUpdate, TSelect}"/> object to allow user to chain statement methods.</returns>
    IUpdate<TUpdate, TSelect> Where(Expression<Func<TUpdate, TSelect, bool>> expression);
    /// <summary>
    /// Specifies a subquery to constrain records from an outer query.  <typeparamref name="TExists"/> represents
    /// the subquery entity, and <typeparamref name="TSelect"/> represents the outer query entity.
    /// </summary>
    /// <typeparam name="TExists">Subquery table entity.</typeparam>
    /// <param name="expression">Expression that defines how the subquery matches rows to the outer query.
    /// Can also include additional filter criteria.</param>
    /// <returns><see cref="IUpdate{TUpdate, TSelect}"/> object to allow user to chain query methods.</returns>
    IUpdate<TUpdate, TSelect> WhereExists<TExists>(Expression<Func<TExists, TSelect, bool>> expression) where TExists : class;
    /// <summary>
    /// Works as opposite of Exists.  Returns a true condition if no rows are returned by the subquery.
    /// <typeparamref name="TExists"/> represents the subquery entity, and <typeparamref name="TSelect"/> represents
    /// the outer query entity.
    /// </summary>
    /// <typeparam name="TExists">Subquery table entity.</typeparam>
    /// <param name="expression">Expression that defines how the subquery matches rows to the outer query.  Can also
    /// include additional filter criteria.</param>
    /// <returns><see cref="IUpdate{TUpdate, TSelect}"/> object to allow user to chain query methods.</returns>
    IUpdate<TUpdate, TSelect> WhereNotExists<TExists>(Expression<Func<TExists, TSelect, bool>> expression) where TExists : class;
    /// <summary>
    /// Apply subquery to WHERE condition for the target column used in the <paramref name="expression"/> parameter.  
    /// An inline func delegate can be used for the <paramref name="subquery"/> parameter.  
    /// Example: <code>() => { return <see cref="SqlWriters.Subquery{TSub}(string, Predicates, Prefix)"/>; }</code>
    /// </summary>
    /// <remarks>
    /// Use <see cref="SqlWriters.Subquery{TSub}(string, Predicates, Prefix)"/> helper method to build subquery statement.
    /// Helper can be used to set the condition type(s).  Such as: And/Or, equals, less than, etc.
    /// </remarks>
    /// <typeparam name="TColumn">Column type.</typeparam>
    /// <param name="expression">Target column.</param>
    /// <param name="subquery">Func delegate.  Must return an <see cref="ISubquery"/> object</param>
    /// <returns><see cref="IUpdate{TUpdate, TSelect}"/> object to allow user to chain statement methods.</returns>
    IUpdate<TUpdate, TSelect> WhereSubquery<TColumn>(Expression<Func<TUpdate, TColumn>> expression, Func<ISubquery> subquery);
}