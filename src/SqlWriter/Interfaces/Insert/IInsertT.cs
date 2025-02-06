using System.Linq.Expressions;

namespace SqlWriter;

/// <summary>
/// Builds a SQL INSERT statement using a SELECT query as the data source.  Inherits <see cref="ISqlStatement"/>.
/// </summary>
/// <typeparam name="TInsert">INSERT table entity.</typeparam>
/// <typeparam name="TSelect">SELECT table entity.</typeparam>
public interface IInsert<TInsert, TSelect> : ISqlStatement where TInsert : class where TSelect : class
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
    /// Concatenates the results of the current statement into the next statement to follow.  In addition, will apply a @@ROWCOUNT
    /// check that will short-circuit the statement if the result of <see cref="IInsert{TInsert, TSelect}"/> did not affect any rows.
    /// </summary>
    /// <remarks>
    /// It is recommended that building of the next statement be done using the fluent build methods on this object.  This helps to 
    /// ensure the last statement in the chain is able to compile all preceding statements into one. If you choose to break up 
    /// the fluent build process, be aware that the value type will be different that the one used to hold the instantiated value.
    /// </remarks>
    /// <returns><see cref="IConcatSql"/> object to allow user to concatenate one or more SQL statements.</returns>
    IConcatSql ConcatWithRowCount();
    /// <summary>
    /// Uses the <typeparamref name="TProjection"/> type to project the columns that will appear in the INSERT statement.
    /// Method will use the order of the public properties returned from the <typeparamref name="TProjection"/>'s GetProperties()
    /// method as the insert ordinal position.
    /// </summary>
    /// <typeparam name="TProjection">Projection model type.</typeparam>
    /// <returns><see cref="IInsert{TInsert, TSelect}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, TSelect> Into<TProjection>() where TProjection : class;
    /// <summary>
    /// Sets the target columns to use in the INTO clause of the statement.
    /// </summary>
    /// <param name="columns">INTO column targets.</param>
    /// <returns><see cref="IInsert{TInsert, TSelect}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, TSelect> Into(Expression<Func<TInsert, object>> columns);
    /// <summary>
    /// Adds an OUTPUT clause to return information from each row affected by the INSERT statement.  Results (INSERTED action)
    /// are saved to the <typeparamref name="TOutputTo"/> table.
    /// </summary>
    /// <remarks>
    /// Useful to retrieve the value of an identity column.
    /// </remarks>
    /// <typeparam name="TOutputTo">OUTPUT INTO entity table.</typeparam>
    /// <param name="statement"><see cref="MemberInitExpression"/> expression that maps inserted columns to output table.</param>
    /// <returns><see cref="IInsert{TInsert, TSelect}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, TSelect> OutputTo<TOutputTo>(Expression<Func<TInsert, TOutputTo>> statement) where TOutputTo : class;
    /// <summary>
    /// Uses the <typeparamref name="TProjection"/> type to project the columns that will appear in the SELECT statement.
    /// Method will use the order of the public properties returned from the <typeparamref name="TProjection"/>'s GetProperties()
    /// method as the insert ordinal position.
    /// </summary>
    /// <typeparam name="TProjection">Projection model type.</typeparam>
    /// <returns><see cref="IInsert{TInsert, TSelect}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, TSelect> Select<TProjection>() where TProjection : class;
    /// <summary>
    /// Sets the insert value source columns to be used in the insert statement.  The ordinal position of the SELECT columns 
    /// should match the intended INSERT table columns.
    /// </summary>
    /// <param name="columns">Target columns.</param>
    /// <returns><see cref="IInsert{TInsert, TSelect}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, TSelect> Select(Expression<Func<TSelect, object>> columns);
    /// <summary>
    /// Specifies the filter condition for the target table(s).  Expects a <see cref="BinaryExpression"/>
    /// expression consisting of one or multiple WHERE conditions. Basic example:
    /// <example><code>(a, b) => a.ID == 1 (and) b.Address == "Westminster"</code></example> 
    /// <see cref="Conditions"/> methods can also be used: 
    /// <example><code>(a, b) => Conditions.In(a.PropertyID, 1, 2, 3)</code></example>
    /// </summary>
    /// <remarks>
    /// Method can be called more than once per statement.
    /// </remarks>
    /// <param name="expression"><see cref="BinaryExpression"/> expression that represents the WHERE criteria.</param>
    /// <returns><see cref="IInsert{TInsert, TSelect}"/> object to allow user to chain query methods.</returns>
    IInsert<TInsert, TSelect> Where(Expression<Func<TSelect, bool>> expression);
    /// <summary>
    /// Specifies a subquery to constrain records from an outer query.  <typeparamref name="TExists"/> represents the
    /// subquery entity, and <typeparamref name="TSelect"/> represents the outer query entity.
    /// </summary>
    /// <typeparam name="TExists">Subquery table entity.</typeparam>
    /// <param name="expression">Expression that defines how the subquery matches rows to the outer query.
    /// Can also include additional filter criteria.</param>
    /// <returns><see cref="IInsert{TInsert, TSelect}"/> object to allow user to chain query methods.</returns>
    IInsert<TInsert, TSelect> WhereExists<TExists>(Expression<Func<TExists, TSelect, bool>> expression) where TExists : class;
    /// <summary>
    /// Works as opposite of Exists.  Returns a true condition if no rows are returned by the subquery.
    /// <typeparamref name="TExists"/> represents the subquery entity, and <typeparamref name="TSelect"/> represents
    /// the outer query entity.
    /// </summary>
    /// <typeparam name="TExists">Subquery table entity.</typeparam>
    /// <param name="expression">Expression that defines how the subquery matches rows to the outer query.
    /// Can also include additional filter criteria.</param>
    /// <returns><see cref="IInsert{TInsert, TSelect}"/> object to allow user to chain query methods.</returns>
    IInsert<TInsert, TSelect> WhereNotExists<TExists>(Expression<Func<TExists, TSelect, bool>> expression) where TExists : class;
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
    /// <returns><see cref="IInsert{TInsert, TSelect}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, TSelect> WhereSubquery<TColumn>(Expression<Func<TSelect, TColumn>> expression, Func<ISubquery> subquery);
    /// <summary>
    /// SELECT statement clause that divides the query result into groups of rows.  Accepts Lambda function to
    /// identify target column.
    /// </summary>
    /// <param name="column">Column.</param>
    /// <typeparam name="TProperty">Column data type.</typeparam>
    /// <returns><see cref="IInsert{TInsert, TSelect}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, TSelect> GroupBy<TProperty>(Expression<Func<TSelect, TProperty>> column);
    /// <summary>
    /// SELECT statement clause that divides the query result into groups of rows.  Accepts an anonymous object to
    /// identify target column or columns.
    /// </summary>
    /// <param name="column">Column.</param>
    /// <returns><see cref="IInsert{TInsert, TSelect}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, TSelect> GroupBy(Expression<Func<TSelect, object>> column);
    /// <summary>
    /// Specifies a search condition for a group or an aggregate.
    /// </summary>
    /// <example><code>(a, b) => SqlFunc.Count() > 2</code></example>
    /// <param name="condition">Having condition.</param>
    /// <returns><see cref="IInsert{TInsert, TSelect}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, TSelect> Having(Expression<Func<TSelect, bool>> condition);
}