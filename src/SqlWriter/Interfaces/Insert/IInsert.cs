using System.Linq.Expressions;

namespace SqlWriter;

/// <summary>
/// Builds a SQL INSERT statement for a single table.  Inherits <see cref="ISqlStatement"/>.
/// </summary>
/// <typeparam name="T">Entity table type.</typeparam>
public interface IInsert<T> : ISqlStatement where T : class
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
    /// check that will short-circuit the statement if the result of <see cref="IInsert{T}"/> did not affect any rows.
    /// </summary>
    /// <remarks>
    /// It is recommended that building of the next statement be done using the fluent build methods on this object.  This helps to 
    /// ensure the last statement in the chain is able to compile all preceding statements into one. If you choose to break up 
    /// the fluent build process, be aware that the value type will be different that the one used to hold the instantiated value.
    /// </remarks>
    /// <returns><see cref="IConcatSql"/> object to allow user to concatenate one or more SQL statements.</returns>
    IConcatSql ConcatWithRowCount();
    IInsert<T> Set<TProperty>(Expression<Func<T, TProperty>> column, TProperty value);
    IInsert<T> SetRaw(string columnName, string columnValue);
    IInsert<T> SetRaw<TProperty>(Expression<Func<T, TProperty>> expression, string columnValue);
    /// <summary>
    /// Apply subquery result to a target column in an INSERT statement.
    /// </summary>
    /// <remarks>
    /// Use <see cref="SqlWriters.Subquery{TSub}(string, Predicates, Prefix)"/> helper method to build subquery statement.
    /// </remarks>
    /// <param name="column">Alias name of subquery.</param>
    /// <param name="subquery">Subquery statement.</param>
    /// <returns><see cref="IInsert{T}"/> object to allow user to chain statement methods.</returns>
    IInsert<T> SetSubquery<TProperty>(Expression<Func<T, TProperty>> column, ISubquery subquery);
    /// <summary>
    /// Apply subquery result to a target column in an INSERT statement.
    /// </summary>
    /// <remarks>
    /// Use <see cref="SqlWriters.Subquery{TSub}(string, Predicates, Prefix)"/> helper method to build subquery statement.
    /// </remarks>
    /// <param name="column">Alias name of subquery.</param>
    /// <param name="subquery">Subquery statement.</param>
    /// <returns><see cref="IInsert{T}"/> object to allow user to chain statement methods.</returns>
    IInsert<T> SetSubquery<TProperty>(Expression<Func<T, TProperty>> column, Func<ISubquery> subquery);
}