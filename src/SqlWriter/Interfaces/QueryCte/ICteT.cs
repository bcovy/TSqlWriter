using System.Linq.Expressions;

namespace SqlWriter;
/// <summary>
/// Builds a SQL Common Table Expression (CTE) statement for a single table.  Inherits <see cref="ICteStatement"/>.
/// </summary>
/// <typeparam name="T">Entity table type.</typeparam>
public interface ICte<T> : ICteStatement where T : class
{
    /// <summary>
    /// Expects a <see cref="NewExpression"/> that consists of an anonymous object of property names which represent the target 
    /// columns and associated table alias name to include in the SELECT clause.  Key property of object can be used to represent 
    /// the alias name of a given column.  In addition, you can also create an alias column with a raw value or SQL function.  Example: 
    /// <example>
    /// <code>(a) => new { a.ID, Address = a.Address1, Alias1 = "hello world" } //will project to: SELECT a.ID, a.Address1 AS [Address], @p0 AS [Alias1]</code>
    /// </example>
    /// </summary>
    /// <remarks>
    /// Method will parameterize input values, and create an associated input parameter.
    /// </remarks>
    /// <param name="columns">Columns to project into statement.</param>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain query methods.</returns>
    ICte<T> Select(Expression<Func<T, object>> columns);
    /// <summary>
    /// Adds column name(s) as raw un-parameterized string value.  If column includes an alias name, item should be added as a single string value: 
    /// <code>"table1.Column1 AS [ColAlias1]"</code>
    /// </summary>
    /// <remarks>
    /// Column name should also include reference to table alias. 
    /// </remarks>
    /// <param name="columns">Column names.</param>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain query methods.</returns>
    ICte<T> Select(params string[] columns);
    /// <summary>
    /// Uses the <typeparamref name="TProjection"/> type to project the columns that will appear in the SELECT statement.   
    /// </summary>
    /// <remarks>
    /// Builder will search <typeparamref name="T"/> entity in the order it was applied during the instantiation period, 
    /// and attach the associated table alias to the first match that is found.  Once a match is found, the column is removed 
    /// from any further searches on additional entities.  This is done to eliminate duplicate column  names in the final condition.  
    /// </remarks>
    /// <typeparam name="TProjection">Projection model type.</typeparam>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain query methods.</returns>
    ICte<T> Select<TProjection>() where TProjection : class;
    /// <summary>
    /// Adds column as a raw un-parameterized string value.
    /// </summary>
    /// <remarks>
    /// Column name should also include reference to table alias. 
    /// </remarks>
    /// <param name="statement">Column name or SQL statement.</param>
    /// <param name="aliasName">Column alias name.</param>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain query methods.</returns>
    ICte<T> SelectRaw(string statement, string aliasName);
    ICte<T> SelectTop(int topValue);
    /// <summary>
    /// Expects a <see cref="BinaryExpression"/> expression consisting of one or multiple WHERE conditions. Basic example:
    /// <example><code>(a) => a.ID == 1 (and) a.Address == "Westminster"</code></example> 
    /// <see cref="Conditions"/> methods can also be used: 
    /// <example><code>(a) => Conditions.In(a.PropertyID, 1, 2, 3)</code></example>
    /// </summary>
    /// <remarks>
    /// Method can be called more than once per statement.
    /// </remarks>
    /// <param name="expression"><see cref="BinaryExpression"/> expression that represents the WHERE criteria.</param>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain query methods.</returns>
    ICte<T> Where(Expression<Func<T, bool>> expression);
    /// <summary>
    /// Specifies a subquery to constrain records from an outer query.  <typeparamref name="TExists"/> represents the subquery entity, 
    /// and <typeparamref name="T"/> represents the outer query entity.
    /// </summary>
    /// <typeparam name="TExists">Subquery table entity.</typeparam>
    /// <param name="expression">Expression that defines how the subquery matches rows to the outer query.  Can also include additional
    /// filter criteria.</param>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain query methods.</returns>
    ICte<T> WhereExists<TExists>(Expression<Func<TExists, T, bool>> expression) where TExists : class;
    /// <summary>
    /// Works as opposite of Exists.  Returns a true condition if no rows are returned by the subquery.  <typeparamref name="TExists"/> represents the subquery entity, 
    /// and <typeparamref name="T"/> represents the outer query entity.
    /// </summary>
    /// <typeparam name="TExists">Subquery table entity.</typeparam>
    /// <param name="expression">Expression that defines how the subquery matches rows to the outer query.  Can also include additional filter criteria.</param>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain query methods.</returns>
    ICte<T> WhereNotExists<TExists>(Expression<Func<TExists, T, bool>> expression) where TExists : class;
    /// <summary>
    /// Sets the next prefix condition to OR.
    /// </summary>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain query methods</returns>
    ICte<T> WhereOr();
    /// <summary>
    /// Apply subquery to WHERE condition for the target column used in the <paramref name="expression"/> parameter.  
    /// An inline func delegate can be used for the <paramref name="subquery"/> parameter.  Example: <code>() => { return <see cref="SqlWriters.Subquery{TSub}(string, Predicates, Prefix)"/>; }</code>
    /// </summary>
    /// <remarks>
    /// Use <see cref="SqlWriters.Subquery{TSub}(string, Predicates, Prefix)"/> helper method to build subquery statement.
    /// Helper can be used to set the condition type(s).  Such as: And/Or, equals, less than, etc.
    /// </remarks>
    /// <typeparam name="TColumn">Column type.</typeparam>
    /// <param name="expression">Target column.</param>
    /// <param name="subquery">Func delegate.  Must return an <see cref="ISubquery"/> object</param>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain query methods.</returns>
    ICte<T> WhereSubquery<TColumn>(Expression<Func<T, TColumn>> expression, Func<ISubquery> subquery);
    /// <summary>
    /// Apply subquery to WHERE condition for the target column used in the <paramref name="expression"/> parameter.
    /// </summary>
    /// <remarks>
    /// Use <see cref="SqlWriters.Subquery{TSub}(string, Predicates, Prefix)"/> helper method to build subquery statement.  
    /// Helper can be used to set the condition type(s).  Such as: And/Or, equals, less than, etc.
    /// </remarks>
    /// <typeparam name="TColumn">Column type.</typeparam>
    /// <param name="expression">WHERE condition column.</param>
    /// <param name="subquery">Subquery statement.</param>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain query methods.</returns>
    ICte<T> WhereSubquery<TColumn>(Expression<Func<T, TColumn>> expression, ISubquery subquery);
    /// <summary>
    /// SELECT statement clause that divides the query result into groups of rows.  Accepts Lambda function to
    /// identify target column.
    /// </summary>
    /// <param name="column">Column.</param>
    /// <typeparam name="TProperty">Column data type.</typeparam>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain statement methods.</returns>
    ICte<T> GroupBy<TProperty>(Expression<Func<T, TProperty>> column);
    /// <summary>
    /// SELECT statement clause that divides the query result into groups of rows.  Accepts an anonymous object to
    /// identify target column or columns.
    /// </summary>
    /// <param name="column">Column.</param>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain statement methods.</returns>
    ICte<T> GroupBy(Expression<Func<T, object>> column);
    /// <summary>
    /// Specifies a search condition for a group or an aggregate.
    /// </summary>
    /// <param name="expression">Having condition.</param>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain statement methods.</returns>
    ICte<T> Having(Expression<Func<T, bool>> expression);
    /// <summary>
    /// Sorts data returned by the query.
    /// </summary>
    /// <param name="column">Sort column.</param>
    /// <param name="direction">Direction: ASC, DESC.</param>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain statement methods.</returns>
    ICte<T> OrderBy(string column, string direction);
    /// <summary>
    /// Sorts data returned by the query in ascending order.
    /// </summary>
    /// <param name="column">Sort column <see cref="MemberExpression"/>.</param>
    /// <typeparam name="TProperty">Column type.</typeparam>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain statement methods.</returns>
    ICte<T> OrderByAsc<TProperty>(Expression<Func<T, TProperty>> column);
    /// <summary>
    /// Sorts data returned by the query in ascending order.
    /// </summary>
    /// <param name="column">Sort column.</param>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain statement methods.</returns>
    ICte<T> OrderByAsc(string column);
    /// <summary>
    /// Sorts data returned by the query in descending order.
    /// </summary>
    /// <param name="column">Sort column <see cref="MemberExpression"/>.</param>
    /// <typeparam name="TProperty">Column type.</typeparam>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain statement methods.</returns>
    ICte<T> OrderByDesc<TProperty>(Expression<Func<T, TProperty>> column);
    /// <summary>
    /// Sorts data returned by the query in descending order.
    /// </summary>
    /// <param name="column">Sort column.</param>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain statement methods.</returns>
    ICte<T> OrderByDesc(string column);
}
