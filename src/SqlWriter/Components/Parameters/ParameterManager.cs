using System.Data;
using Microsoft.Data.SqlClient;
using SqlWriter.Components.Tables;
using SqlWriter.Infrastructure;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Components.Parameters;

public class ParameterManager : IParameterManager
{
    private int _parameterCounter;

    public List<IParameterModel> Parameters { get; } = [];
    public SqlParameter[] GetSqlParameters => Parameters.Select(x => x.GetSqlDataParameter).ToArray();
    public IDictionary<string, object> GetParameters => Parameters
        .ToDictionary(k => k.ParameterNameRaw, v => v.Value);

    public string Add<TParam>(TParam value, string parameterName = "p")
    {
        string sqlParamName = $"{parameterName}{_parameterCounter}";
        SqlDbType sqlType = value.TranslateSqlDbType();
        SqlParameter sqlParameter = new(sqlParamName, sqlType) { Value = Equals(value, default(TParam)) ? DBNull.Value : value };

        return AddParameter(new ParameterModel<TParam>(value, sqlParamName, sqlParameter));
    }

    public string Add<TParam>(ColumnModel column, TParam value, string parameterName = "p")
    {
        string sqlParamName = $"{parameterName}{_parameterCounter}";
        SqlParameter sqlParameter = new(sqlParamName, column.SqlDataType)
        {
            Value = Equals(value, default(TParam)) ? DBNull.Value : value,
            Precision = (byte)column.Precision,
            Scale = (byte)column.Scale,
            Size = column.Size
        };

        return AddParameter(new ParameterModel<TParam>(value, sqlParamName, sqlParameter));
    }

    public void AddParameter<TParam>(TParam value, string parameterName, SqlParameter sqlParameter)
    {
        Parameters.Add(new ParameterModel<TParam>(value, parameterName, sqlParameter));
    }

    public void AddParameters(IEnumerable<IParameterModel> parameter)
    {
        Parameters.AddRange(parameter);
    }

    private string AddParameter(IParameterModel parameter)
    {
        Parameters.Add(parameter);
        _parameterCounter += 1;

        return parameter.ParameterName;
    }
}