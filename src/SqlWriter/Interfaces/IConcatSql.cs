using SqlWriter.Interfaces.Internals;

namespace SqlWriter;

public interface IConcatSql
{
    IParameterManager ParameterManager { get; }
    string SqlStatement { get; }
    string ParameterPrefix { get; }
}
