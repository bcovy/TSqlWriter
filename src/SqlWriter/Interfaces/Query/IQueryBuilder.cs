using System.Linq.Expressions;

namespace SqlWriter;

/// <summary>
/// Builds a SQL query statement using a combination of Entity classes and string user inputs.  Inherits <see cref="ISqlStatement"/>.
/// </summary>
public interface IQueryBuilder : ISqlStatement
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
    /// <summary>
    /// Apply target columns to SELECT clause of statement.  Column names should include table alias.
    /// Example format: [TableAlias].[Column].
    /// </summary>
    /// <param name="columns">Columns with table alias.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder Select(params string[] columns);
    /// <summary>
    /// Create inner join condition between two table entities.
    /// </summary>
    /// <param name="columns"><see cref="BinaryExpression"/> that represents a SQL join condition between
    /// one or two key pair(s).</param>
    /// <param name="joinTableAlias">Join table alias name.</param>
    /// <typeparam name="TParentTable">Parent entity.</typeparam>
    /// <typeparam name="TJoinTable">Child join entity.</typeparam>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder Join<TParentTable, TJoinTable>(Expression<Func<TParentTable, TJoinTable, bool>> columns, string joinTableAlias) where TParentTable : class where TJoinTable : class;
    /// <summary>
    /// Create left outer join condition between two table entities.
    /// </summary>
    /// <param name="columns"><see cref="BinaryExpression"/> that represents a SQL join condition between
    /// one or two key pair(s).</param>
    /// <param name="joinTableAlias">Join table alias name.</param>
    /// <typeparam name="TParentTable">Parent entity.</typeparam>
    /// <typeparam name="TJoinTable">Child join entity.</typeparam>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder JoinLeft<TParentTable, TJoinTable>(Expression<Func<TParentTable, TJoinTable, bool>> columns, string joinTableAlias) where TParentTable : class where TJoinTable : class;
    /// <summary>
    /// Create right outer join condition between two table entities.
    /// </summary>
    /// <param name="columns"><see cref="BinaryExpression"/> that represents a SQL join condition between
    /// one or two key pair(s).</param>
    /// <param name="joinTableAlias">Join table alias name.</param>
    /// <typeparam name="TParentTable">Parent entity.</typeparam>
    /// <typeparam name="TJoinTable">Child join entity.</typeparam>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder JoinRight<TParentTable, TJoinTable>(Expression<Func<TParentTable, TJoinTable, bool>> columns, string joinTableAlias) where TParentTable : class where TJoinTable : class;
    /// <summary>
    /// Joins a common table expression (CTE) to the current query statement as an Inner Join.
    /// </summary>
    /// <remarks>
    /// <typeparamref name="TTable"/> represents the entity table for which the CTE statement will use as the parent
    /// join target.  This method assumes <typeparamref name="TTable"/> and CTE statement share the same column name
    /// used in the <paramref name="joinColumnName"/> member expression.
    /// </remarks>
    /// <typeparam name="TTable">Parent join table.</typeparam>
    /// <typeparam name="TProperty">Parent join column type.</typeparam>
    /// <param name="joinColumnName"><see cref="MemberExpression"/> of target column.  Assumes matching name is
    /// present in SELECT statement of CTE.</param>
    /// <param name="cteStatement">CTE statement.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder With<TTable, TProperty>(Expression<Func<TTable, TProperty>> joinColumnName, ICteStatement cteStatement) where TTable : class;
    /// <summary>
    /// Joins a common table expression (CTE) to the current query statement as an Inner Join.
    /// </summary>
    /// <remarks>
    /// <typeparamref name="TTable"/> represents the entity table for which the CTE statement will use as the Parent/Right join target.
    /// <typeparamref name="TCteJoinTable"/> represents the entity used to form the column(s) that make up the Right side of the join.
    /// </remarks>
    /// <typeparam name="TTable">Parent join table.</typeparam>
    /// <typeparam name="TCteJoinTable">CTE Entity used to represent Right side join column(s).</typeparam>
    /// <param name="joinExpression">Join expression.  Expects an expression type of <see cref="BinaryExpression"/>. 
    /// Expression can be either a standard join type, or a composite of two columns on both the left and right sides.</param>
    /// <param name="cteStatement">CTE statement.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder With<TTable, TCteJoinTable>(Expression<Func<TTable, TCteJoinTable, bool>> joinExpression, ICteStatement cteStatement) where TTable : class where TCteJoinTable : class;
    /// <summary>
    /// Joins a common table expression (CTE) to the current query statement as a Left Outer Join.
    /// </summary>
    /// <remarks>
    /// <typeparamref name="TTable"/> represents the entity table for which the CTE statement will use as the Parent/Right join target.
    /// <typeparamref name="TCteJoinTable"/> represents the entity used to form the column(s) that make up the Right side of the join.
    /// </remarks>
    /// <typeparam name="TTable">Parent join table.</typeparam>
    /// <typeparam name="TCteJoinTable">CTE Entity used to represent Right side join column(s).</typeparam>
    /// <param name="joinExpression">Join expression.  Expects an expression type of <see cref="BinaryExpression"/>.  
    /// Expression can be either a standard join type, or a composite of two columns on both the left and right sides.</param>
    /// <param name="cteStatement">CTE statement</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder WithLeft<TTable, TCteJoinTable>(Expression<Func<TTable, TCteJoinTable, bool>> joinExpression, ICteStatement cteStatement) where TTable : class where TCteJoinTable : class;
    /// <summary>
    /// Joins a common table expression (CTE) to the current query statement as a Left Outer Join.
    /// </summary>
    /// <remarks>
    /// <typeparamref name="TTable"/> represents the entity table for which the CTE statement will use as the parent
    /// join target.  This method assumes <typeparamref name="TTable"/> and CTE statement share the same column name
    /// used in the <paramref name="joinColumnName"/> member expression.
    /// </remarks>
    /// <typeparam name="TTable">Parent join table.</typeparam>
    /// <typeparam name="TProperty">Parent join column type.</typeparam>
    /// <param name="joinColumnName">Parent join column expression.</param>
    /// <param name="cteStatement">CTE statement.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder WithLeft<TTable, TProperty>(Expression<Func<TTable, TProperty>> joinColumnName, ICteStatement cteStatement) where TTable : class;
    /// <summary>
    /// Joins a common table expression (CTE) to the current query statement a Right Outer Join.
    /// </summary>
    /// <remarks>
    /// <typeparamref name="TTable"/> represents the entity table for which the CTE statement will use as the parent
    /// join target.  This method assumes <typeparamref name="TTable"/> and CTE statement share the same column name
    /// used in the <paramref name="joinColumnName"/> member expression.
    /// </remarks>
    /// <typeparam name="TTable">Parent join table.</typeparam>
    /// <typeparam name="TProperty">Parent join column type.</typeparam>
    /// <param name="joinColumnName">Parent join column expression.</param>
    /// <param name="cteStatement">CTE statement.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder WithRight<TTable, TProperty>(Expression<Func<TTable, TProperty>> joinColumnName, ICteStatement cteStatement) where TTable : class;
    /// <summary>
    /// Sets the next prefix condition to OR.
    /// </summary>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder WhereOr();
    /// <summary>
    /// Specifies a subquery to constrain records from an outer query.  <typeparamref name="TExists"/> represents the
    /// subquery entity, and <typeparamref name="TOuter"/> represents the outer query entity.
    /// </summary>
    /// <typeparam name="TExists">Subquery table entity.</typeparam>
    /// <typeparam name="TOuter">Outer query table.</typeparam>
    /// <param name="expression">Expression that defines how the subquery matches rows to the outer query.  Can also
    /// include additional filter criteria.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder WhereExists<TExists, TOuter>(Expression<Func<TExists, TOuter, bool>> expression) where TExists : class where TOuter : class;
    /// <summary>
    /// Works as opposite of Exists.  Returns a true condition if no rows are returned by the subquery.
    /// <typeparamref name="TExists"/> represents the subquery entity, and <typeparamref name="TOuter"/> represents
    /// the outer query entity.
    /// </summary>
    /// <typeparam name="TExists">Subquery table entity.</typeparam>
    /// <typeparam name="TOuter">Outer query table.</typeparam>
    /// <param name="expression">Expression that defines how the subquery matches rows to the outer query.  Can also
    /// include additional filter criteria.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder WhereNotExists<TExists, TOuter>(Expression<Func<TExists, TOuter, bool>> expression) where TExists : class where TOuter : class;
    /// <summary>
    /// Specifies the search condition for the rows returned by the query.  Use a lambda expression to build a
    /// strongly typed WHERE condition for the <typeparamref name="TTable"/> entity.
    /// </summary>
    /// <remarks>
    /// Method assumes <typeparamref name="TTable"/> entity is decorated with <see cref="TableNameAttribute"/>.  
    /// Will apply WHERE condition regardless of <see cref="Builders.Query.QueryBuilder.DynamicWhere"/> setting.
    /// </remarks>
    /// <typeparam name="TTable">Entity table type.</typeparam>
    /// <param name="expression">Where condition expression.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder Where<TTable>(Expression<Func<TTable, bool>> expression) where TTable : class;
    /// <summary>
    /// Specifies the search condition for the rows returned by the query.  Use a lambda expression to build a strongly
    /// typed WHERE condition for the <typeparamref name="TTable1"/> and <typeparamref name="TTable2"/> entities.
    /// </summary>
    /// <remarks>
    /// Method assumes <typeparamref name="TTable1"/> and <typeparamref name="TTable2"/> entities are decorated
    /// with <see cref="TableNameAttribute"/>.  Will apply WHERE condition regardless of
    /// <see cref="Builders.Query.QueryBuilder.DynamicWhere"/> setting.
    /// </remarks>
    /// <typeparam name="TTable1">Entity table type.</typeparam>
    /// <typeparam name="TTable2">Entity table type.</typeparam>
    /// <param name="expression">Where condition expression.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder Where<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> expression) where TTable1 : class where TTable2 : class;
    /// <summary>
    /// Specifies the search condition for the rows returned by the query.
    /// </summary>
    /// <remarks>
    /// If <see cref="Builders.Query.QueryBuilder.DynamicWhere"/> was set to true on instantiation, the condition will
    /// not be applied if <paramref name="value"/> is null.
    /// </remarks>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="column">Target column.  Should include associated table alias name.</param>
    /// <param name="value">Filter value.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder Where<T>(string column, T value);
    /// <summary>
    /// Apply BETWEEN function to WHERE condition.
    /// </summary>
    /// <remarks>
    /// Will apply WHERE condition regardless of <see cref="Builders.Query.QueryBuilder.DynamicWhere"/> setting.
    /// </remarks>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="column">Target column.  Should include associated table alias name.</param>
    /// <param name="value1">Start value.</param>
    /// <param name="value2">End value.</param>
    /// <returns></returns>
    IQueryBuilder WhereBetween<T>(string column, T value1, T value2);
    /// <summary>
    /// Apply not equal operator to WHERE condition.
    /// </summary>
    /// <remarks>
    /// If <see cref="Builders.Query.QueryBuilder.DynamicWhere"/> was set to true on instantiation, the condition will
    /// not be applied if <paramref name="value"/> is null.
    /// </remarks>
    /// <param name="column">Target column.  Should include associated table alias name.</param>
    /// <param name="value">Condition value.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder WhereNotEqual<T>(string column, T value);
    /// <summary>
    /// Apply less than operator to WHERE condition.
    /// </summary>
    /// <remarks>
    /// If <see cref="Builders.Query.QueryBuilder.DynamicWhere"/> was set to true on instantiation, the condition will
    /// not be applied if <paramref name="value"/> is null.
    /// </remarks>
    /// <param name="column">Target column.  Should include associated table alias name.</param>
    /// <param name="value">Condition value.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder WhereLessThan<T>(string column, T value);
    /// <summary>
    /// Apply less than or equal to operator to WHERE condition.
    /// </summary>
    /// <remarks>
    /// If <see cref="Builders.Query.QueryBuilder.DynamicWhere"/> was set to true on instantiation, the condition will
    /// not be applied if <paramref name="value"/> is null.
    /// </remarks>
    /// <param name="column">Target column.  Should include associated table alias name.</param>
    /// <param name="value">Condition value.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder WhereLessThanEqual<T>(string column, T value);
    /// <summary>
    /// Apply greater than operator to WHERE condition.
    /// </summary>
    /// <remarks>
    /// If <see cref="Builders.Query.QueryBuilder.DynamicWhere"/> was set to true on instantiation, the condition will
    /// not be applied if <paramref name="value"/> is null.
    /// </remarks>
    /// <param name="column">Target column.  Should include associated table alias name.</param>
    /// <param name="value">Condition value.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder WhereGreaterThan<T>(string column, T value);
    /// <summary>
    /// Apply greater than or equal to operator to WHERE condition.
    /// </summary>
    /// <remarks>
    /// If <see cref="Builders.Query.QueryBuilder.DynamicWhere"/> was set to true on instantiation, the condition will
    /// not be applied if <paramref name="value"/> is null.
    /// </remarks>
    /// <param name="column">Target column.  Should include associated table alias name.</param>
    /// <param name="value">Condition value.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder WhereGreaterThanEqual<T>(string column, T value);
    /// <summary>
    /// Apply LIKE function to WHERE condition.
    /// </summary>
    /// <remarks>
    /// If <see cref="Builders.Query.QueryBuilder.DynamicWhere"/> was set to true on instantiation, the condition will
    /// not be applied if <paramref name="value"/> is null.
    /// </remarks>
    /// <param name="column">Target column.  Should include associated table alias name.</param>
    /// <param name="value">Condition value.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder WhereLike(string column, string value);
    /// <summary>
    /// Apply NOT LIKE function to WHERE condition.
    /// </summary>
    /// <remarks>
    /// If <see cref="Builders.Query.QueryBuilder.DynamicWhere"/> was set to true on instantiation, the condition will
    /// not be applied if <paramref name="value"/> is null.
    /// </remarks>
    /// <param name="column">Target column.  Should include associated table alias name.</param>
    /// <param name="value">Condition value.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder WhereNotLike(string column, string value);
    /// <summary>
    /// Apply subquery to WHERE condition for the target column used in the <paramref name="column"/> parameter.  
    /// An inline func delegate can be used for the <paramref name="subquery"/> parameter.  
    /// Example: <code>() => { return <see cref="SqlWriters.Subquery{TSub}(string, Predicates, Prefix)"/>; }</code>
    /// </summary>
    /// <remarks>
    /// Use <see cref="SqlWriters.Subquery{TSub}(string, Predicates, Prefix)"/> helper method to build subquery statement.
    /// Helper can be used to set the condition type(s).  Such as: And/Or, equals, less than, etc.
    /// </remarks>
    /// <param name="column">Target column.  Should include associated table alias name.</param>
    /// <param name="subquery">Func delegate.  Must return an <see cref="ISubquery"/> object</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder WhereSubquery(string column, Func<ISubquery> subquery);
    /// <summary>
    /// Apply subquery to WHERE condition for the target column used in the <paramref name="column"/> parameter.
    /// </summary>
    /// <remarks>
    /// Use <see cref="SqlWriters.Subquery{TSub}(string, Predicates, Prefix)"/> helper method to build subquery statement.  
    /// Helper can be used to set the condition type(s).  Such as: And/Or, equals, less than, etc.
    /// </remarks>
    /// <param name="column">Target column.  Should include associated table alias name.</param>
    /// <param name="subquery">Subquery statement.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder WhereSubquery(string column, ISubquery subquery);
    /// <summary>
    /// SELECT statement clause that divides the query result into groups of rows. 
    /// </summary>
    /// <remarks>
    /// Method can be used multiple times if statement requires additional GROUP BY columns.
    /// </remarks>
    /// <param name="column">Column name with alias.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder GroupBy(string column);
    /// <summary>
    /// Specifies a search condition for a group or an aggregate.
    /// </summary>
    /// <param name="expression">Having condition.</param>
    /// <typeparam name="TTable">Target table entity.</typeparam>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder Having<TTable>(Expression<Func<TTable, bool>> expression) where TTable : class;
    /// <summary>
    /// Sorts data returned by the query.
    /// </summary>
    /// <param name="column">Sort column.</param>
    /// <param name="direction">Sort direction: ASC or DESC.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder OrderBy(string column, string direction);
    /// <summary>
    /// Sorts data returned by the query in ascending order.
    /// </summary>
    /// <param name="column">Sort column.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder OrderByAsc(string column);
    /// <summary>
    /// Sorts data returned by the query in descending order.
    /// </summary>
    /// <param name="column">Sort column.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder OrderByDesc(string column);
    /// <summary>
    /// Apply OFFSET and FETCH clauses to limit the number of rows produced in the query results set.
    /// Allows results to be paged to limit the number of rows sent to the client.
    /// </summary>
    /// <param name="pageIndex">Page to display.</param>
    /// <param name="pageSize">Amount of rows per page.</param>
    /// <returns><see cref="IQueryBuilder"/> object to allow user to chain query methods.</returns>
    IQueryBuilder Pager(int pageIndex, int pageSize);
}