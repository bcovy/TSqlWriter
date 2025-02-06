using System.Data;

namespace SqlWriter;
/// <summary>
/// Container for a raw SQL statement and associated parameters.  Inherits <see cref="ISqlStatement"/>.
/// </summary>
public interface IRawSql : ISqlStatement
{
    /// <summary>
    /// Concatenates the results of the current statement into the next statement to follow.
    /// </summary>
    /// <remarks>
    /// It is recommended that building of the next statement be done using the fluent build methods on this object.  This helps to 
    /// ensure the last statement in the chain is able to compile all preceding statements into one. If you choose to break up 
    /// the fluent build process, be aware that the value type will be different that the one used to hold the instantiated value.
    /// </remarks>
    /// <returns><see cref="IConcatSql"/> object to allow user to concatenate one or more SQL statements.</returns>
    IConcatSql Concat();
    /// <summary>
    /// Creates a parameterized value and adds it to the <see cref="ISqlStatement.Parameters"/> collection.
    /// </summary>
    /// <remarks>
    /// <paramref name="parameterName"/> arguement should match what is used in SQL statement, and not include the
    /// @ character.  Method will infer <see cref="SqlDbType"/> from <typeparamref name="TParam"/>.
    /// </remarks>
    /// <param name="value">Parameter value.</param>
    /// <param name="parameterName">Parameter name.  Should not include @ character.</param>
    /// <typeparam name="TParam">Parameter type.</typeparam>
    /// <returns><see cref="IRawSql"/> object to allow user to concatenate one or more SQL statements.</returns>
    IRawSql AddParameter<TParam>(TParam value, string parameterName);
    /// <summary>
    /// Creates a parameterized value and adds it to the <see cref="ISqlStatement.Parameters"/> collection.
    /// </summary>
    /// <remarks>
    /// <paramref name="parameterName"/> arguement should match what is used in SQL statement, and not include the
    /// @ character.
    /// </remarks>
    /// <param name="value">Parameter value.</param>
    /// <param name="parameterName">Parameter name.  Should not include @ character.</param>
    /// <param name="dbType">SQL Server specific data type.</param>
    /// <typeparam name="TParam">Parameter type.</typeparam>
    /// <returns><see cref="IRawSql"/> object to allow user to concatenate one or more SQL statements.</returns>
    IRawSql AddParameter<TParam>(TParam value, string parameterName, SqlDbType dbType);
}