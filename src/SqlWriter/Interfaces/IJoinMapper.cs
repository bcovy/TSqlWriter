using System.Linq.Expressions;
using SqlWriter.Components.Joins;

namespace SqlWriter.Interfaces;

/// <summary>
/// Represents the Join types and condition to implement when compiling a SQL statement.
/// </summary>
public interface IJoinMapper
{
    List<JoinMap> JoinMaps { get; }
    /// <summary>
    /// Inner Join tables <typeparamref name="TTable1"/> and <typeparamref name="TTable2"/> using Primary Key field
    /// identified in <see cref="TableNameAttribute"/>. By convention, method will assume Primary Key name in
    /// <typeparamref name="TTable1"/> has an associated field name in <typeparamref name="TTable2"/>.
    /// </summary>
    /// <typeparam name="TTable1">Join table one.  Expects entity to have Primary Key identified in attribute.</typeparam>
    /// <typeparam name="TTable2">Join table two.</typeparam>
    /// <returns><see cref="IJoinMapper"/> object to allow user to chain query methods.</returns>
    IJoinMapper Inner<TTable1, TTable2>() where TTable1 : class where TTable2 : class;
    /// <summary>
    /// Creates an inner join between the <typeparamref name="TTable1"/> and <typeparamref name="TTable2"/> entities.
    /// </summary>
    /// <typeparam name="TTable1">First table.</typeparam>
    /// <typeparam name="TTable2">Second table.</typeparam>
    /// <param name="joinExpression">Expects a <see cref="BinaryExpression"/> that represents a SQL column join condition.
    /// Allows up to two conditions.</param>
    /// <returns><see cref="IJoinMapper"/> object to allow user to chain methods.</returns>
    IJoinMapper Inner<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> joinExpression) where TTable1 : class where TTable2 : class;
    /// <summary>
    /// Left Outer Join tables <typeparamref name="TTable1"/> and <typeparamref name="TTable2"/> using Primary Key field
    /// identified in <see cref="TableNameAttribute"/>. By convention, method will assume Primary Key name in
    /// <typeparamref name="TTable1"/> has an associated field name in <typeparamref name="TTable2"/>.
    /// </summary>
    /// <typeparam name="TTable1">Join table one.  Expects entity to have Primary Key identified in attribute.</typeparam>
    /// <typeparam name="TTable2">Join table two.</typeparam>
    /// <returns><see cref="IJoinMapper"/> object to allow user to chain query methods.</returns>
    IJoinMapper Left<TTable1, TTable2>() where TTable1 : class where TTable2 : class;
    /// <summary>
    /// Creates a left outer join between the <typeparamref name="TTable1"/> and <typeparamref name="TTable2"/> entities.
    /// </summary>
    /// <typeparam name="TTable1">First table.</typeparam>
    /// <typeparam name="TTable2">Second table.</typeparam>
    /// <param name="joinExpression">Expects a <see cref="BinaryExpression"/> that represents a SQL column join condition.
    /// Allows up to two conditions.</param>
    /// <returns><see cref="IJoinMapper"/> object to allow user to chain methods.</returns>
    IJoinMapper Left<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> joinExpression) where TTable1 : class where TTable2 : class;
    /// <summary>
    /// Right Outer Join tables <typeparamref name="TTable1"/> and <typeparamref name="TTable2"/> using Primary Key field
    /// identified in <see cref="TableNameAttribute"/>.  By convention, method will assume Primary Key name in
    /// <typeparamref name="TTable1"/> has an associated field name in <typeparamref name="TTable2"/>.
    /// </summary>
    /// <typeparam name="TTable1">Join table one.  Expects entity to have Primary Key identified in attribute.</typeparam>
    /// <typeparam name="TTable2">Join table two.</typeparam>
    /// <returns><see cref="IJoinMapper"/> object to allow user to chain query methods.</returns>
    IJoinMapper Right<TTable1, TTable2>() where TTable1 : class where TTable2 : class;
    /// <summary>
    /// Creates a right outer join between the <typeparamref name="TTable1"/> and <typeparamref name="TTable2"/> entities.
    /// </summary>
    /// <typeparam name="TTable1">First table.</typeparam>
    /// <typeparam name="TTable2">Second table.</typeparam>
    /// <param name="joinExpression">Expects a <see cref="BinaryExpression"/> that represents a SQL column join condition.
    /// Allows up to two conditions.</param>
    /// <returns><see cref="IJoinMapper"/> object to allow user to chain methods.</returns>
    IJoinMapper Right<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> joinExpression) where TTable1 : class where TTable2 : class;
}