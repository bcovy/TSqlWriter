using SqlWriter.Interfaces.Internals;

namespace SqlWriter;

public interface ICteStatement
{
    IEnumerable<IParameterModel> Parameters { get; }
    string CteAlias { get; }
    bool StopColumnProjection { get; }
    bool IncludeJoinColumn { get; }
    IEnumerable<string> Columns { get; }
    /// <summary>
    /// Compiles user inputs into a SQL statement.
    /// </summary>
    /// <remarks>
    /// Method should be called before accessing <see cref="Columns"/> property, as method
    /// may apply Select All condition if no column info was supplied by the user.
    /// </remarks>
    /// <returns>SQL string statement.</returns>
    string CompileStatement();
}