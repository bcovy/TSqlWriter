using System.Linq.Expressions;

namespace SqlWriter;

/// <summary>
/// Builds a SQL DELETE statement for a single table.  Inherits <see cref="ISqlStatement"/>.
/// </summary>
/// <typeparam name="T">Entity table type.</typeparam>
public interface IDelete<T> : ISqlStatement where T : class
{
    /// <summary>
    /// Specifies the filter condition for the target table.
    /// </summary>
    /// <param name="expression"><see cref="BinaryExpression"/> expression that represents the WHERE criteria.</param>
    /// <returns><see cref="IDelete{T}"/> object to allow user to chain query methods.</returns>
    IDelete<T> Where(Expression<Func<T, bool>> expression);
}