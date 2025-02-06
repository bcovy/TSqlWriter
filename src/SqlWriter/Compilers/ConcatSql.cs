using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Compilers;

public class ConcatSql : IConcatSql
{
    public IParameterManager ParameterManager { get; }
    public string SqlStatement { get; }
    public string ParameterPrefix { get; }

    public ConcatSql(IParameterManager parameterManager, string sqlStatement, string parameterPrefix)
    {
        ParameterManager = parameterManager;
        SqlStatement = sqlStatement;
        ParameterPrefix = parameterPrefix;
    }
}
