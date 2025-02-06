using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Compilers;

public class UnionSql(IParameterManager parameterManager, string sqlStatement, string parameterPrefix) : IUnion
{
    public IParameterManager ParameterManager { get; } = parameterManager;
    public string SqlStatement { get; } = sqlStatement;
    public string ParameterPrefix { get; } = parameterPrefix;
}