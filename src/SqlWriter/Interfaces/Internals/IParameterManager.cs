using Microsoft.Data.SqlClient;
using SqlWriter.Components.Tables;

namespace SqlWriter.Interfaces.Internals;

public interface IParameterManager
{
    /// <summary>
    /// Returns a list of <see cref="IParameterModel"/> objects.
    /// </summary>
    List<IParameterModel> Parameters { get; }
    /// <summary>
    /// Returns key/value a pair of parameters, with key value representing the parameter name, and value the
    /// parameter value.
    /// </summary>
    /// <remarks>
    /// Parameter name omits @ character.
    /// </remarks>
    IDictionary<string, object> GetParameters { get; }
    /// <summary>
    /// Returns an array of <see cref="SqlParameter"/> objects.
    /// </summary>
    SqlParameter[] GetSqlParameters { get;  }
    /// <summary>
    /// Adds parameter to <see cref="Parameters"/> collection, and returns unique parameter name consisting of
    /// <paramref name="parameterName"/> with parameter counter suffix.  Method will try to derive corresponding
    /// <see cref="SqlParameter"/> type from <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="value">Parameter value.</param>
    /// <param name="parameterName">Parameter name @ character omitted.</param>
    /// <typeparam name="TParam">Parameter value type.</typeparam>
    /// <returns>Parameter name with @ character prefix and parameter counter suffix.</returns>
    string Add<TParam>(TParam value, string parameterName = "p");
    /// <summary>
    /// Adds parameter to <see cref="Parameters"/> collection, and returns unique parameter name consisting of
    /// <paramref name="parameterName"/> with parameter counter suffix.  
    /// </summary>
    /// <param name="column"><see cref="ColumnModel"/> used to derive parameter meta data information.</param>
    /// <param name="value">Parameter value.</param>
    /// <param name="parameterName">Parameter name @ character omitted.</param>
    /// <typeparam name="TParam">Parameter value type..</typeparam>
    /// <returns>Parameter name with @ character prefix and parameter counter suffix.</returns>
    string Add<TParam>(ColumnModel column, TParam value, string parameterName = "p");
    /// <summary>
    /// Uses argument inputs to create a <see cref="IParameterModel"/> object to add to the internal <see cref="Parameters"/>
    /// collection.  Method will not increment internal parameter name counter.
    /// </summary>
    /// <remarks>
    /// Method is mainly meant for external use by consumer in the event they want to use the internal components for an
    /// ad hoc SQL builder.
    /// </remarks>
    /// <param name="value">Parameter value.</param>
    /// <param name="parameterName">Parameter name with @ character omitted.</param>
    /// <param name="sqlParameter">SqlParameter object.</param>
    /// <typeparam name="TParam">Parameter value type.</typeparam>
    void AddParameter<TParam>(TParam value, string parameterName, SqlParameter sqlParameter);
    /// <summary>
    /// Adds the elements of the specified collection to the end of the <see cref="Parameters"/>.
    /// Method will not increment internal parameter name counter.
    /// </summary>
    /// <param name="parameter">Collection to be added.</param>
    void AddParameters(IEnumerable<IParameterModel> parameter);
}
