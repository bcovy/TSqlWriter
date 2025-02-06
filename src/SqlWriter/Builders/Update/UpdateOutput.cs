#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace SqlWriter;
/// <summary>
/// Model that represents the OUTPUT clause, used to return information based on each row 
/// affected by an INSERT or DELETE statement.
/// </summary>
/// <typeparam name="T">Entity type to write results to.</typeparam>
public class UpdateOutput<T> where T : class
{
    /// <summary>
    /// Specifies the values deleted by the update or delete statement.
    /// </summary>
    public T Deleted { get; set; }

    /// <summary>
    /// Specifies the values added by the insert or update statement.
    /// </summary>
    public T Inserted { get; set; }
}
