using SqlWriter.Interfaces.Internals;

namespace SqlWriter;

public interface IUnion
{
    IParameterManager ParameterManager { get; }
    string SqlStatement { get; }
    string ParameterPrefix { get; }
}