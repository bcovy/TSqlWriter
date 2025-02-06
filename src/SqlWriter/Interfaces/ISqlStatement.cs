using Microsoft.Data.SqlClient;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter;
/// <summary>
/// Represents the base functionality for building a SQL statement, with associated parameters.
/// </summary>
public interface ISqlStatement
{
    /// <summary>
    /// Returns Fully-qualified SQL statement.
    /// </summary>
    /// <returns>Fully-qualified SQL statement.</returns>
    string GetSqlStatement();
    /// <summary>
    /// Collection of parameters as <see cref="IParameterModel"/> types.
    /// </summary>
    IEnumerable<IParameterModel> Parameters { get; }
    /// <summary>
    /// Collection of parameters as <see cref="SqlParameter"/> types.
    /// </summary>
    SqlParameter[] GetSqlParameters { get; }
    /// <summary>
    /// Collection of parameters represented as key/value pairs.
    /// </summary>
    IDictionary<string, object> GetParameters { get; }
}