using System.Linq.Expressions;
using SqlWriter.Interfaces;

namespace SqlWriter;

/// <summary>
/// Builds a SQL INSERT statement using a SELECT query as the data source.  Inherits <see cref="ISqlStatement"/>.
/// </summary>
/// <typeparam name="TInsert">INSERT table entity.</typeparam>
/// <typeparam name="T">SELECT table entity.</typeparam>
/// <typeparam name="T2">SELECT table entity.</typeparam>
/// <typeparam name="T3">SELECT table entity.</typeparam>
/// <typeparam name="T4">SELECT table entity.</typeparam>
public interface IInsert<TInsert, T, T2, T3, T4> : ISqlStatement where TInsert : class where T : class where T2 : class where T3 : class where T4 : class
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
    /// check that will short-circuit the statement if the result of <see cref="IInsert{TInsert, T, T2, T3, T4}"/> did not affect any rows.
    /// </summary>
    /// <remarks>
    /// It is recommended that building of the next statement be done using the fluent build methods on this object.  This helps to 
    /// ensure the last statement in the chain is able to compile all preceding statements into one. If you choose to break up 
    /// the fluent build process, be aware that the value type will be different that the one used to hold the instantiated value.
    /// </remarks>
    /// <returns><see cref="IConcatSql"/> object to allow user to concatenate one or more SQL statements.</returns>
    IConcatSql ConcatWithRowCount();
    /// <summary>
    /// Sets the target columns to use in the INTO clause of the statement.
    /// </summary>
    /// <param name="columns">INTO target columns.</param>
    /// <returns><see cref="IInsert{TInsert, T, T2, T3, T4}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, T, T2, T3, T4> Into(Expression<Func<TInsert, object>> columns);
    /// <summary>
    /// Adds an OUTPUT clause to return information from each row affected by the INSERT statement.  Results (INSERTED action)
    /// are saved to the <typeparamref name="TOutputTo"/> table.
    /// </summary>
    /// <remarks>
    /// Useful to retrieve the value of an identity column.
    /// </remarks>
    /// <typeparam name="TOutputTo">OUTPUT INTO entity table.</typeparam>
    /// <param name="statement"><see cref="MemberInitExpression"/> expression that maps inserted columns to output table.</param>
    /// <returns><see cref="IInsert{TInsert, T, T2, T3, T4}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, T, T2, T3, T4> OutputTo<TOutputTo>(Expression<Func<TInsert, TOutputTo>> statement) where TOutputTo : class;
    /// <summary>
    /// Sets the insert value source columns to be used in the insert statement.  The ordinal position of the SELECT columns 
    /// should match the intended INSERT table columns.
    /// </summary>
    /// <param name="columns">Target columns.</param>
    /// <returns><see cref="IInsert{TInsert, T, T2, T3, T4}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, T, T2, T3, T4> Select(Expression<Func<T, T2, T3, T4, object>> columns);
    /// <summary>
    /// Join tables <typeparamref name="TTable1"/> and <typeparamref name="TTable2"/> using Primary Key field identified
    /// in <see cref="TableNameAttribute"/>.
    /// By convention, method will assume Primary Key name in <typeparamref name="TTable1"/> has an associated field name
    /// in <typeparamref name="TTable2"/>.
    /// </summary>
    /// <typeparam name="TTable1">Join table one.  Expects entity to have Primary Key identified in attribute.</typeparam>
    /// <typeparam name="TTable2">Join table two.</typeparam>
    /// <param name="joinType">Join type.</param>
    /// <returns><see cref="IInsert{TInsert, T, T2, T3, T4}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, T, T2, T3, T4> Join<TTable1, TTable2>(JoinType joinType = JoinType.Inner) where TTable1 : class where TTable2 : class;
    /// <summary>
    /// Identify the table join statements using the <see cref="IJoinMapper"/> action delegate interface.
    /// Example: <code> join => join.Inner&lt;Table1, Table2>((a, b)) => a.ID == b.ID); </code>
    /// </summary>
    /// <param name="mapper">Action delegate used to map table joins.</param>
    /// <returns><see cref="IInsert{TInsert, T, T2, T3, T4}"/> object to allow user to chain statement methods.</returns>
    /// <remarks>
    /// This method allows the user to identify all join mappings for the associated entity tables.  
    /// Method should only be called once per query statement.
    /// </remarks>
    IInsert<TInsert, T, T2, T3, T4> Join(Action<IJoinMapper> mapper);
    /// <summary>
    /// Create join condition between two table entities.
    /// </summary>
    /// <param name="columns">Expects a <see cref="BinaryExpression"/> that represents a SQL join condition between
    /// one or two key pair(s).</param>
    /// <param name="joinType">Join type.</param>
    /// <returns><see cref="IInsert{TInsert, T, T2, T3, T4}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, T, T2, T3, T4> Join(Expression<Func<T, T2, T3, T4, bool>> columns, JoinType joinType);
    /// <summary>
    /// Create inner join condition between two table entities.
    /// </summary>
    /// <param name="columns">Expects a <see cref="BinaryExpression"/> that represents a SQL join condition between
    /// one or two key pair(s).</param>
    /// <returns><see cref="IInsert{TInsert, T, T2, T3, T4}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, T, T2, T3, T4> JoinInner(Expression<Func<T, T2, T3, T4, bool>> columns);
    /// <summary>
    /// Create left outer join condition between two table entities.
    /// </summary>
    /// <param name="columns">Expects a <see cref="BinaryExpression"/> that represents a SQL join condition between
    /// one or two key pair(s).</param>
    /// <returns><see cref="IInsert{TInsert, T, T2, T3, T4}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, T, T2, T3, T4> JoinLeftOuter(Expression<Func<T, T2, T3, T4, bool>> columns);
    /// <summary>
    /// Specifies the filter condition for the target table(s).  Expects a <see cref="BinaryExpression"/> expression
    /// consisting of one or multiple WHERE conditions. Basic example:
    /// <example><code>(a, b, c, d) => a.ID == 1 (and) b.Address == "Westminster"</code></example> 
    /// <see cref="Conditions"/> methods can also be used: 
    /// <example><code>(a, b, c, d) => Conditions.In(a.PropertyID, 1, 2, 3)</code></example>
    /// </summary>
    /// <remarks>
    /// Method can be called more than once per statement.
    /// </remarks>
    /// <param name="expression"><see cref="BinaryExpression"/> expression that represents the WHERE criteria.</param>
    /// <returns><see cref="IInsert{TInsert, T, T2, T3, T4}"/> object to allow user to chain query methods.</returns>
    IInsert<TInsert, T, T2, T3, T4> Where(Expression<Func<T, T2, T3, T4, bool>> expression);
    /// <summary>
    /// Specifies a subquery to constrain records from an outer query.  <typeparamref name="TExists"/> represents the
    /// subquery entity, and <typeparamref name="TOuter"/> represents the outer query entity.
    /// </summary>
    /// <typeparam name="TExists">Subquery table entity.</typeparam>
    /// <typeparam name="TOuter">Outer query table.</typeparam>
    /// <param name="expression">Expression that defines how the subquery matches rows to the outer query.  Can also
    /// include additional filter criteria.</param>
    /// <returns><see cref="IInsert{TInsert, T, T2, T3, T4}"/> object to allow user to chain query methods.</returns>
    IInsert<TInsert, T, T2, T3, T4> WhereExists<TExists, TOuter>(Expression<Func<TExists, TOuter, bool>> expression) where TExists : class where TOuter : class;
    /// <summary>
    /// Works as opposite of Exists.  Returns a true condition if no rows are returned by the subquery.
    /// <typeparamref name="TExists"/> represents the subquery entity, and <typeparamref name="TOuter"/> represents
    /// the outer query entity.
    /// </summary>
    /// <typeparam name="TExists">Subquery table entity.</typeparam>
    /// <typeparam name="TOuter">Outer query table.</typeparam>
    /// <param name="expression">Expression that defines how the subquery matches rows to the outer query.  Can also
    /// include additional filter criteria.</param>
    /// <returns><see cref="IInsert{TInsert, T, T2}"/> object to allow user to chain query methods.</returns>
    IInsert<TInsert, T, T2, T3, T4> WhereNotExists<TExists, TOuter>(Expression<Func<TExists, TOuter, bool>> expression) where TExists : class where TOuter : class;
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
    /// <returns><see cref="IInsert{TInsert, T, T2, T3, T4}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, T, T2, T3, T4> WhereSubquery<TColumn>(Expression<Func<T, T2, T3, T4, TColumn>> expression, Func<ISubquery> subquery);
    /// <summary>
    /// SELECT statement clause that divides the query result into groups of rows.  Accepts Lambda function to
    /// identify target column.
    /// </summary>
    /// <param name="column">Column.</param>
    /// <typeparam name="TProperty">Column data type.</typeparam>
    /// <returns><see cref="IInsert{TInsert, T, T2, T3, T4}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, T, T2, T3, T4> GroupBy<TProperty>(Expression<Func<T, T2, T3, T4, TProperty>> column);
    /// <summary>
    /// SELECT statement clause that divides the query result into groups of rows.  Accepts an anonymous object to
    /// identify target column or columns.
    /// </summary>
    /// <param name="column">Column.</param>
    /// <returns><see cref="IInsert{TInsert, T, T2, T3, T4}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, T, T2, T3, T4> GroupBy(Expression<Func<T, T2, T3, T4, object>> column);
    /// <summary>
    /// Specifies a search condition for a group or an aggregate.
    /// </summary>
    /// <example><code>(a, b) => SqlFunc.Count() > 2</code></example>
    /// <param name="condition">Having condition.</param>
    /// <returns><see cref="IInsert{TInsert, T, T2, T3, T4}"/> object to allow user to chain statement methods.</returns>
    IInsert<TInsert, T, T2, T3, T4> Having(Expression<Func<T, T2, T3, T4, bool>> condition);
}