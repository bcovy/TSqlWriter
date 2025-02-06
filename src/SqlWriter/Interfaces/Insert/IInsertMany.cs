
namespace SqlWriter;

/// <summary>
/// Builds a SQL INSERT statement for a single table with multiple insert values.  Inherits <see cref="ISqlStatement"/>.
/// </summary>
/// <typeparam name="T">Entity table type.</typeparam>
public interface IInsertMany<in T> : ISqlStatement where T : class
{
    /// <summary>
    /// Apply a single row of INSERT values to the statement.  Method will use <typeparamref name="T"/> to 
    /// match value with target column.
    /// </summary>
    /// <remarks>
    /// Object will not parameterize any values.  As such, the <see cref="IInsertMany{T}"/> object should not be 
    /// used with user/client supplied data.  Empty/null entity values will be replaced with the SQL equivalent: NULL. 
    /// </remarks>
    /// <param name="entity">Insert values.</param>
    /// <returns><see cref="IInsertMany{T}"/> object to allow user to chain statement methods.</returns>
    IInsertMany<T> SetValues(T entity);
}