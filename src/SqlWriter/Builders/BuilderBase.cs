using Microsoft.Data.SqlClient;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Builders;

public abstract class BuilderBase(IParameterManager parameterManager)
{
    protected IParameterManager ParameterManager { get; } = parameterManager;
    public IEnumerable<IParameterModel> Parameters => ParameterManager.Parameters;
    public SqlParameter[] GetSqlParameters => ParameterManager.GetSqlParameters;
    public IDictionary<string, object> GetParameters => ParameterManager.GetParameters;
}