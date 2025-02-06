using Microsoft.Data.SqlClient;
using System.Data;

namespace SqlWriter.Interfaces.Internals;

public interface IParameterModel
{
    /// <summary>
    /// Parameter name with @ character.
    /// </summary>
    string ParameterName { get; }
    /// <summary>
    /// Parameter name without @ character.
    /// </summary>
    string ParameterNameRaw { get; }
    object Value { get; }
    SqlDbType SqlDataType { get; }
    SqlParameter GetSqlDataParameter { get; }
}