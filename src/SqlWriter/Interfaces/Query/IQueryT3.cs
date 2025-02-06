using System.Linq.Expressions;
using SqlWriter.Interfaces;

namespace SqlWriter;

/// <summary>
/// Builds a SQL query statement from 3 tables.  Inherits <see cref="ISqlStatement"/>.
/// </summary>
/// <typeparam name="T">Entity table type.</typeparam>
/// <typeparam name="T2">Entity table type.</typeparam>
/// <typeparam name="T3">Entity table type.</typeparam>
public interface IQuery<T, T2, T3> : ISqlStatement where T : class where T2 : class where T3 : class
{
    /// <summary>
    /// Returns <see cref="IUnion"/> to allow the concatenation of multiple SELECT statements.
    /// Union will remove duplicate results and provide a unique data set.
    /// </summary>
    /// <returns><see cref="IUnion"/> object to allow user to chain query methods.</returns>
    IUnion Union();
    /// <summary>
    /// Returns <see cref="IUnion"/> to allow the concatenation of multiple SELECT statements.
    /// Union All will return all rows, including duplicates.
    /// </summary>
    /// <returns><see cref="IUnion"/> object to allow user to chain query methods.</returns>
    IUnion UnionAll();
    IQuery<T, T2, T3> SelectAll();
    /// <summary>
    /// Expects a <see cref="NewExpression"/> that consists of an anonymous object of property names which represent the target 
    /// columns and associated table alias name to include in the SELECT clause.  Key property of object can be used to represent 
    /// the alias name of a given column.  In addition, you can also create an alias column with a raw value or SQL function.  Example: 
    /// <example>
    /// <code>(a, b, c) => new { a.ID, Address = b.Address1, c.City, Alias1 = "hello world" } //will project to: SELECT a.ID, b.Address1 AS [Address], c.City, @p0 AS [Alias1]</code>
    /// </example>
    /// </summary>
    /// <remarks>
    /// Method will parameterize an input values in the statement, and create an associated input parameter.
    /// </remarks>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain query methods.</returns>
    IQuery<T, T2, T3> Select(Expression<Func<T, T2, T3, object>> columns);
    /// <summary>
    /// Adds column name(s) as raw un-parameterized string value.  If column includes an alias name, item should be added as a single string value: 
    /// <code>"table1.Column1 AS [ColAlias1]"</code>
    /// </summary>
    /// <remarks>
    /// Column name should also include reference to table alias. 
    /// </remarks>
    /// <param name="columns">Column names.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain query methods.</returns>
    IQuery<T, T2, T3> Select(params string[] columns);
    /// <summary>
    /// Uses the <typeparamref name="TProjection"/> type to project the columns that will appear in the SELECT statement.   
    /// </summary>
    /// <remarks>
    /// Builder will search <typeparamref name="T"/>, <typeparamref name="T2"/> and <typeparamref name="T3"/> entities in the order it was applied during the 
    /// instantiation period, and attach the associated table alias to the first match that is found.  Once a match is found, the 
    /// column is removed from any further searches on additional entities.  This is done to eliminate duplicate column names in the final condition.  
    /// </remarks>
    /// <typeparam name="TProjection">Projection model type.</typeparam>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain query methods.</returns>
    IQuery<T, T2, T3> Select<TProjection>() where TProjection : class;
    /// <summary>
    /// Adds column as a raw un-parameterized string value.
    /// </summary>
    /// <remarks>
    /// Column name should also include reference to table alias. 
    /// </remarks>
    /// <param name="statement">Column name or SQL statement.</param>
    /// <param name="aliasName">Column alias name.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain query methods.</returns>
    IQuery<T, T2, T3> SelectRaw(string statement, string aliasName);
    IQuery<T, T2, T3> SelectTop(int topValue);
    /// <summary>
    /// Apply subquery to the SELECT statement results, using an inline func delegate.  
    /// Example: <code>() => { return <see cref="SqlWriters.Subquery{TSub}(string, Predicates, Prefix)"/>; }</code>
    /// </summary>
    /// <remarks>
    /// Use <see cref="SqlWriters.Subquery{TSub}(string, Predicates, Prefix)"/> helper method to build subquery statement.
    /// </remarks>
    /// <param name="columnName">Alias name of subquery.</param>
    /// <param name="subquery">Func delegate.  Must return an <see cref="ISubquery"/> object.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain query methods.</returns>
    IQuery<T, T2, T3> SelectSubquery(string columnName, Func<ISubquery> subquery);
    /// <summary>
    /// Join tables <typeparamref name="TTable1"/> and <typeparamref name="TTable2"/> using Primary Key field identified in <see cref="TableNameAttribute"/>.
    /// By convention, method will assume Primary Key name in <typeparamref name="TTable1"/> has an associated field name in <typeparamref name="TTable2"/>.
    /// </summary>
    /// <typeparam name="TTable1">Join table one.  Expects entity to have Primary Key identified in attribute.</typeparam>
    /// <typeparam name="TTable2">Join table two.</typeparam>
    /// <param name="joinType">Join type.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain query methods.</returns>
    IQuery<T, T2, T3> Join<TTable1, TTable2>(JoinType joinType = JoinType.Inner) where TTable1 : class where TTable2 : class;
    /// <summary>
    /// Identify the table join statements using the <see cref="IJoinMapper"/> action delegate interface.
    /// Example: <code> join => join.Inner&lt;Table1, Table2>((a, b)) => a.ID == b.ID); </code>
    /// </summary>
    /// <param name="mapper">Action delegate used to map table joins.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain query methods.</returns>
    /// <remarks>
    /// This method allows the user to identify all join mappings for the associated entity tables.  
    /// Method should only be called once per query statement.
    /// </remarks>
    IQuery<T, T2, T3> Join(Action<IJoinMapper> mapper);
    /// <summary>
    /// Create join condition between two table entities.  
    /// </summary>
    /// <param name="columns">Expects a <see cref="BinaryExpression"/> that represents a SQL join condition between one
    /// or two key pair(s).</param>
    /// <param name="joinType">Join type.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain statement methods.</returns>
    IQuery<T, T2, T3> Join(Expression<Func<T, T2, T3, bool>> columns, JoinType joinType);
    /// <summary>
    /// Create inner join condition between two table entities. 
    /// </summary>
    /// <param name="columns">Expects a <see cref="BinaryExpression"/> that represents a SQL join condition between one
    /// or two key pair(s).</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain statement methods.</returns>
    IQuery<T, T2, T3> JoinInner(Expression<Func<T, T2, T3, bool>> columns);
    /// <summary>
    /// Create left outer join condition between two table entities. 
    /// </summary>
    /// <param name="columns">Expects a <see cref="BinaryExpression"/> that represents a SQL join condition between one
    /// or two key pair(s).</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain statement methods.</returns>
    IQuery<T, T2, T3> JoinLeftOuter(Expression<Func<T, T2, T3, bool>> columns);
    /// <summary>
    /// Joins a common table expression (CTE) to the current query statement as an Inner Join.
    /// </summary>
    /// <remarks>
    /// <typeparamref name="T"/>, <typeparamref name="T2"/>, or <typeparamref name="T3"/> represents the entity table
    /// for which the CTE statement will use as the parent join target.  This method assumes <typeparamref name="T"/>,
    /// <typeparamref name="T2"/>, or <typeparamref name="T3"/>, and CTE statement share the same column name used in
    /// the <paramref name="joinColumnName"/> member expression.
    /// </remarks>
    /// <typeparam name="TProperty">Parent join column type.</typeparam>
    /// <param name="joinColumnName">Parent join column expression.</param>
    /// <param name="cteStatement">CTE statement.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain query methods.</returns>
    IQuery<T, T2, T3> With<TProperty>(Expression<Func<T, T2, T3, TProperty>> joinColumnName, ICteStatement cteStatement);
    /// <summary>
    /// Joins a common table expression (CTE) to the current query statement as an Inner Join.
    /// </summary>
    /// <remarks>
    /// <typeparamref name="T"/>, <typeparamref name="T2"/>, or <typeparamref name="T3"/> represent the entity table for which the CTE 
    /// statement will use as the Parent/Right join target.  <typeparamref name="TCteJoinTable"/> represents the entity used to form the column(s) 
    /// that make up the Right side of the join.
    /// </remarks>
    /// <typeparam name="TCteJoinTable">CTE Entity used to represent Right side join column(s).</typeparam>
    /// <param name="joinExpression">Join expression.  Expects an expression type of <see cref="BinaryExpression"/>. 
    /// Expression can be either a standard join type, or a composite of two columns on both the left and right sides.</param>
    /// <param name="cteStatement">CTE statement.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain query methods.</returns>
    IQuery<T, T2, T3> With<TCteJoinTable>(Expression<Func<T, T2, T3, TCteJoinTable, bool>> joinExpression, ICteStatement cteStatement) where TCteJoinTable : class;
    /// <summary>
    /// Joins a common table expression (CTE) to the current query statement as a Left Outer Join.
    /// </summary>
    /// <remarks>
    /// <typeparamref name="T"/>, <typeparamref name="T2"/>, or <typeparamref name="T3"/> represents the entity table
    /// for which the CTE statement will use as the parent join target.  This method assumes <typeparamref name="T"/>,
    /// <typeparamref name="T2"/>, or <typeparamref name="T3"/>, and CTE statement share the same column name used in
    /// the <paramref name="joinColumnName"/> member expression.
    /// </remarks>
    /// <typeparam name="TProperty">Parent join column type.</typeparam>
    /// <param name="joinColumnName">Parent join column expression.</param>
    /// <param name="cteStatement">CTE statement.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain query methods.</returns>
    IQuery<T, T2, T3> WithLeft<TProperty>(Expression<Func<T, T2, T3, TProperty>> joinColumnName, ICteStatement cteStatement);
    /// <summary>
    /// Joins a common table expression (CTE) to the current query statement as a Left Outer Join.
    /// </summary>
    /// <remarks>
    /// <typeparamref name="T"/>, <typeparamref name="T2"/>, or <typeparamref name="T3"/> represent the entity table for which the CTE 
    /// statement will use as the Parent/Right join target.  <typeparamref name="TCteJoinTable"/> represents the entity used to form the column(s) 
    /// that make up the Right side of the join.
    /// </remarks>
    /// <typeparam name="TCteJoinTable">CTE Entity used to represent Right side join column(s).</typeparam>
    /// <param name="joinExpression">Join expression.  Expects an expression type of <see cref="BinaryExpression"/>. 
    /// Expression can be either a standard join type, or a composite of two columns on both the left and right sides.</param>
    /// <param name="cteStatement">CTE statement.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain query methods.</returns>
    IQuery<T, T2, T3> WithLeft<TCteJoinTable>(Expression<Func<T, T2, T3, TCteJoinTable, bool>> joinExpression, ICteStatement cteStatement) where TCteJoinTable : class;
    /// <summary>
    /// Expects a <see cref="BinaryExpression"/> expression consisting of one or multiple WHERE conditions. Basic example:
    /// <example><code>(a, b, c) => a.ID == 1 (and) b.Address == "Westminster"</code></example> 
    /// <see cref="Conditions"/> methods can also be used: 
    /// <example><code>(a, b, c) => Conditions.In(a.PropertyID, 1, 2, 3)</code></example>
    /// </summary>
    /// <remarks>
    /// Method can be called more than once per statement.
    /// </remarks>
    /// <param name="expression"><see cref="BinaryExpression"/> expression that represents the WHERE criteria.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain query methods.</returns>
    IQuery<T, T2, T3> Where(Expression<Func<T, T2, T3, bool>> expression);
    /// <summary>
    /// Specifies a subquery to constrain records from an outer query.  <typeparamref name="TExists"/> represents the subquery entity, and <typeparamref name="TOuter"/>
    /// represents the outer query entity.
    /// </summary>
    /// <typeparam name="TExists">Subquery table entity.</typeparam>
    /// <typeparam name="TOuter">Outer query table.</typeparam>
    /// <param name="expression">Expression that defines how the subquery matches rows to the outer query.  Can also include additional filter criteria.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain query methods.</returns>
    IQuery<T, T2, T3> WhereExists<TExists, TOuter>(Expression<Func<TExists, TOuter, bool>> expression) where TExists : class where TOuter : class;
    /// <summary>
    /// Works as opposite of Exists.  Returns a true condition if no rows are returned by the subquery.  <typeparamref name="TExists"/> represents the subquery entity, 
    /// and <typeparamref name="TOuter"/> represents the outer query entity.
    /// </summary>
    /// <typeparam name="TExists">Subquery table entity.</typeparam>
    /// <typeparam name="TOuter">Outer query table.</typeparam>
    /// <param name="expression">Expression that defines how the subquery matches rows to the outer query.  Can also include additional filter criteria.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain query methods.</returns>
    IQuery<T, T2, T3> WhereNotExists<TExists, TOuter>(Expression<Func<TExists, TOuter, bool>> expression) where TExists : class where TOuter : class;
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
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain query methods.</returns>
    IQuery<T, T2, T3> WhereSubquery<TColumn>(Expression<Func<T, T2, T3, TColumn>> expression, Func<ISubquery> subquery);
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
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain query methods.</returns>
    IQuery<T, T2, T3> WhereSubquery<TColumn>(Expression<Func<T, T2, T3, TColumn>> expression, ISubquery subquery);
    /// <summary>
    /// SELECT statement clause that divides the query result into groups of rows.  Accepts Lambda function to
    /// identify target column.
    /// </summary>
    /// <param name="column">Column.</param>
    /// <typeparam name="TProperty">Column data type.</typeparam>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain statement methods.</returns>
    IQuery<T, T2, T3> GroupBy<TProperty>(Expression<Func<T, T2, T3, TProperty>> column);
    /// <summary>
    /// SELECT statement clause that divides the query result into groups of rows.  Accepts an anonymous object to
    /// identify target column or columns.
    /// </summary>
    /// <param name="column">Column.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain statement methods.</returns>
    IQuery<T, T2, T3> GroupBy(Expression<Func<T, T2, T3, object>> column);
    /// <summary>
    /// Specifies a search condition for a group or an aggregate.
    /// </summary>
    /// <param name="expression">Having condition.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain statement methods.</returns>
    IQuery<T, T2, T3> Having(Expression<Func<T, T2, T3, bool>> expression);
    /// <summary>
    /// Sorts data returned by the query.
    /// </summary>
    /// <param name="column">Sort column.</param>
    /// <param name="direction">Direction: ASC, DESC.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain statement methods.</returns>
    IQuery<T, T2, T3> OrderBy(string column, string direction);
    /// <summary>
    /// Sorts data returned by the query in ascending order.
    /// </summary>
    /// <param name="column">Sort column <see cref="MemberExpression"/>.</param>
    /// <typeparam name="TProperty">Column type.</typeparam>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain statement methods.</returns>
    IQuery<T, T2, T3> OrderByAsc<TProperty>(Expression<Func<T, T2, T3, TProperty>> column);
    /// <summary>
    /// Sorts data returned by the query in ascending order.
    /// </summary>
    /// <param name="column">Sort column.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain statement methods.</returns>
    IQuery<T, T2, T3> OrderByAsc(string column);
    /// <summary>
    /// Sorts data returned by the query in descending order.
    /// </summary>
    /// <param name="column">Sort column <see cref="MemberExpression"/>.</param>
    /// <typeparam name="TProperty">Column type.</typeparam>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain statement methods.</returns>
    IQuery<T, T2, T3> OrderByDesc<TProperty>(Expression<Func<T, T2, T3, TProperty>> column);
    /// <summary>
    /// Sorts data returned by the query in descending order.
    /// </summary>
    /// <param name="column">Sort column.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain statement methods.</returns>
    IQuery<T, T2, T3> OrderByDesc(string column);
    /// <summary>
    /// Apply OFFSET and FETCH clauses to limit the number of rows produced in the query results set.
    /// Allows results to be paged to limit the number of rows sent to the client.
    /// </summary>
    /// <param name="pageIndex">Page to display.</param>
    /// <param name="pageSize">Amount of rows per page.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain query methods.</returns>
    IQuery<T, T2, T3> Pager(int pageIndex, int pageSize);
}