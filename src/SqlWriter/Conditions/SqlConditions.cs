namespace SqlWriter;

public static class Conditions
{
#pragma warning disable IDE0060
    public static bool Between<T>(T column, int start, int end) => true;
    public static bool Between<T>(T column, DateTime start, DateTime end) => true;
    public static bool Between<T>(T column, DateOnly start, DateOnly end) => true;
    /// <summary>
    /// Creates a grouping operator that will evaluate all expressions in the group before the resulting expression is combined with another.  Example:
    /// <code>
    /// (a) => Conditions.Group(a.PropertyID == 12 | a.LastName == "Hello world");  //translates to: WHERE (a.PropertyID = @p0 OR a.LastName = @p1)
    /// </code>
    /// </summary>
    /// <param name="expression">Filter expression operators.</param>
    /// <returns>Default value of true.</returns>
    public static bool Group(bool expression) => true;
    public static bool In<T>(T column, params DateOnly[] values) => true;
    public static bool In<T>(T column, params int[] values) => true;
    public static bool In(string column, params string[] values) => true;
    public static bool IsNull<T>(T column) => true;
    public static bool IsNotNull<T>(T column) => true;
    public static bool NotIn<T>(T column, params DateOnly[] values) => true;
    public static bool NotIn<T>(T column, params int[] values) => true;
    public static bool NotIn(string column, params string[] values) => true;
    public static bool Like<T>(T column, string value) => true;
    /// <summary>
    /// Apply LIKE filter condition.  Use <paramref name="operation"/> to set filter type to: contains, starts, or ends.
    /// </summary>
    /// <typeparam name="T">Column type.</typeparam>
    /// <param name="column">Target column.</param>
    /// <param name="value">Filter value.</param>
    /// <param name="operation">Valid input: contains, starts, ends.</param>
    /// <returns>Default value of true.</returns>
    public static bool Like<T>(T column, string value, string operation) => true;
    /// <summary>
    /// Represents a fully qualified SQL statement.
    /// </summary>
    /// <param name="statement">SQL statement.</param>
    /// <returns>Default value of true.</returns>
    public static bool ParameterizedSql(string statement) => true;
    /// <summary>
    /// Represents a parameterized SQL statement with an associated parameter value.  
    /// The name of the parameter used in the <paramref name="statement"/> arguement must match the entity 
    /// column used in the <paramref name="column"/> arguement in order for the writer to be able to associate 
    /// the value with the parameterized statement.  Example: 
    /// <example><c>DATEDIFF(DAY, a.PcoeDate, GETDATE()) >= @PcoeDate</c></example>
    /// </summary>
    /// <typeparam name="T">Column and value type.</typeparam>
    /// <param name="statement">Parameterized statement.</param>
    /// <param name="column">Source column that will be used to identify parameter type.  Column name must 
    /// match parameter used in <paramref name="statement"/> SQL statement.</param>
    /// <param name="value">Parameter value.</param>
    /// <returns>Default value of true.</returns>
    public static bool ParameterizedSql<T>(string statement, T column, T value) => true;

#pragma warning restore IDE0060
}