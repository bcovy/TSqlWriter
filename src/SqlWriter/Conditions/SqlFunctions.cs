#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable IDE0060
namespace SqlWriter;

public static class SqlFunc
{
    public static int Average<T>(T column) => 0;
    /// <summary>
    /// Returns a string resulting from the concatenation of two or more values.  Numeric values will be represented as an un-quoted string.
    /// All other values will be single quoted.  Requires a minimum of two input values; otherwise, CONCAT will raise an error on the server.
    /// </summary>
    /// <remarks>
    /// Method also accepts string interpolation function.
    /// <code>
    /// SqlFunc.Concat($"hello {fieldInt} ", "world", "!!!")
    /// </code>
    /// </remarks>
    /// <param name="values">Concatenation values.</param>
    /// <returns>Default value of true.</returns>
    public static string Concat(params string[] values) => string.Empty;
    public static T Cast<T>(T column, string result) => default;
    /// <summary>
    /// Applies COUNT(*) function that returns the cardinality in the results set, which includes rows comprised of Null and duplicate values.
    /// </summary>
    /// <returns>Default value of 0.</returns>
    public static int Count() => 0;
    /// <summary>
    /// Applies COUNT([expression]) function that returns the number of non-null values.
    /// </summary>
    /// <typeparam name="T">Property/column type.</typeparam>
    /// <param name="column">Target column name.</param>
    /// <returns>Default value of 0.</returns>
    public static int Count<T>(T column) => 0;
    /// <summary>
    /// Applies COUNT(*) OVER() function.
    /// </summary>
    /// <returns>Default value of true.</returns>
    public static bool CountOver() => true;
    public static TDate EoMonth<TDate>(TDate date) => default;
    public static int DateDiff<TDate, T2Date>(string datePart, TDate startDate, T2Date endDate) => 0;
    public static T IIF<T>(bool expression, T thenValue, T elseValue) => default;
    public static int Max<T>(T column) => 0;
    public static int Min<T>(T column) => 0;
    /// <summary>
    /// Applies raw value of <paramref name="statement"/> as un-parameterized, un-quoted string.
    /// </summary>
    /// <remarks>
    /// Method also accepts string interpolation function.
    /// </remarks>
    /// <param name="statement">Raw SQL statement.</param>
    /// <returns>Default value of true.</returns>
    public static bool RawSql(string statement) => true;
    public static int Round<T>(T column, int length) => 0;
    public static int Sum<T>(T column) => 0;
    public static int Sum(bool column) => 0;

#pragma warning restore IDE0060
}