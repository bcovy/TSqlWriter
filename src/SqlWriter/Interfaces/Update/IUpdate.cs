using System.Linq.Expressions;

namespace SqlWriter;

/// <summary>
/// Builds a SQL UPDATE statement for a single table.  Inherits <see cref="ISqlStatement"/>.
/// </summary>
/// <typeparam name="T">Entity table type.</typeparam>
public interface IUpdate<T> : ISqlStatement where T : class
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
    /// <returns><see cref="IUpdate{T}"/> object to allow user to chain statement methods.</returns>
    IUpdate<T> Set<TProperty>(Expression<Func<T, TProperty>> column, TProperty value);
    /// <summary>
    /// Specify column to update using a function to encapsulate a value.  Use this method if you want value to
    /// include a <see cref="SqlFunc"/> result.
    /// </summary>
    /// <typeparam name="TProperty">Column type.</typeparam>
    /// <param name="column">Target column.</param>
    /// <param name="statement">Function statement.</param>
    /// <returns><see cref="IUpdate{T}"/> object to allow user to chain statement methods.</returns>
    IUpdate<T> Set<TProperty>(Expression<Func<T, TProperty>> column, Expression<Func<T, TProperty>> statement);
    /// <summary>
    /// Specify column to update to <see langword="null"/>.
    /// </summary>
    /// <param name="column">Target column expression.</param>
    /// <typeparam name="TProperty">Column type.</typeparam>
    /// <returns><see cref="IUpdate{T}"/> object to allow user to chain statement methods.</returns>
    IUpdate<T> SetNull<TProperty>(Expression<Func<T, TProperty>> column);
    /// <summary>
    /// Specify column to update using an un-parameterized string value.
    /// </summary>
    /// <param name="columnName">Column name.</param>
    /// <param name="columnValue">Update value.</param>
    /// <returns><see cref="IUpdate{T}"/> object to allow user to chain statement methods.</returns>
    IUpdate<T> SetRaw(string columnName, string columnValue);
    /// <summary>
    /// Specify column to update using an un-parameterized string value.
    /// </summary>
    /// <typeparam name="TProperty">Column type.</typeparam>
    /// <param name="expression">Target column expression.</param>
    /// <param name="columnValue">Update value.</param>
    /// <returns><see cref="IUpdate{T}"/> object to allow user to chain statement methods.</returns>
    IUpdate<T> SetRaw<TProperty>(Expression<Func<T, TProperty>> expression, string columnValue);
    /// <summary>
    /// Apply subquery result to a target column in a SET statement.
    /// </summary>
    /// <remarks>
    /// Use <see cref="SqlWriters.Subquery{TSub}(string, Predicates, Prefix)"/> helper method to build subquery statement.
    /// </remarks>
    /// <param name="column">Alias name of subquery.</param>
    /// <param name="subquery">Subquery statement.</param>
    /// <returns><see cref="IUpdate{T}"/> object to allow user to chain statement methods.</returns>
    IUpdate<T> SetSubquery<TProperty>(Expression<Func<T, TProperty>> column, ISubquery subquery);
    /// <summary>
    /// Apply subquery result to a target column in a SET statement, using an inline func delegate.  
    /// Example: <code>() => { return <see cref="SqlWriters.Subquery{TSub}(string, Predicates, Prefix)"/>; }</code>
    /// </summary>
    /// <remarks>
    /// Use <see cref="SqlWriters.Subquery{TSub}(string, Predicates, Prefix)"/> helper method to build subquery statement.
    /// </remarks>
    /// <typeparam name="TProperty">Column type.</typeparam>
    /// <param name="column">Target update column property.</param>
    /// <param name="subquery">Func delegate.  Must return an <see cref="ISubquery"/> object.</param>
    /// <returns><see cref="IUpdate{T}"/> object to allow user to chain statement methods.</returns>
    IUpdate<T> SetSubquery<TProperty>(Expression<Func<T, TProperty>> column, Func<ISubquery> subquery);
    /// <summary>
    /// Specify the search condition(s) as a function expression.
    /// </summary>
    /// <param name="expression">Search condition expression.</param>
    /// <returns><see cref="IUpdate{T}"/> object to allow user to chain statement methods.</returns>
    IUpdate<T> Where(Expression<Func<T, bool>> expression);
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
    /// <returns><see cref="IUpdate{T}"/> object to allow user to chain statement methods.</returns>
    IUpdate<T> WhereSubquery<TColumn>(Expression<Func<T, TColumn>> expression, Func<ISubquery> subquery);
}