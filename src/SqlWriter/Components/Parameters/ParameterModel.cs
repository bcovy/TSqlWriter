using Microsoft.Data.SqlClient;
using SqlWriter.Interfaces.Internals;
using System.Data;

namespace SqlWriter.Components.Parameters;

public class ParameterModel<T> : IParameterModel
{
    private readonly T _value;
    
    public string ParameterName { get; }
    public string ParameterNameRaw => ParameterName[1..];
    public object Value => _value!;
    public SqlDbType SqlDataType { get; }
    public SqlParameter GetSqlDataParameter { get; }

    public ParameterModel(T value, string parameterName, SqlParameter sqlParameter)
    {
        _value = value;
        ParameterName = $"@{parameterName}";
        GetSqlDataParameter = sqlParameter;
        SqlDataType = sqlParameter.SqlDbType;
    }

    public ParameterModel(T value, string parameterName, SqlDbType sqlDbType)
    {
        _value = value;
        ParameterName = $"@{parameterName}";
        SqlDataType = sqlDbType;
        GetSqlDataParameter = new(parameterName, sqlDbType) { Value = Equals(value, default(T)) ? DBNull.Value : value };
    }
}