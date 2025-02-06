using System.Data;
using System.Reflection;

namespace SqlWriter.Infrastructure;

public static class TranslateDbType
{
    /// <summary>
    /// Returns the associated SqlDbType for the current instance.  A default value of <see cref="SqlDbType.NChar"/> will be 
    /// returned if no match is found.
    /// </summary>
    /// <typeparam name="T">Instance type.</typeparam>
    /// <param name="type">Instance object</param>
    /// <returns>Associated SqlDbType for the current instance.</returns>
    public static SqlDbType TranslateSqlDbType<T>(this T type)
    {
        string typeName = type switch
        {
            PropertyInfo property => Nullable.GetUnderlyingType(property.PropertyType)?.Name ?? property.PropertyType.Name,
            not null => type.GetType().Name,
            _ => Nullable.GetUnderlyingType(typeof(T))?.Name ?? typeof(T).Name
        };

        return typeName switch
        {
            "Byte" => SqlDbType.TinyInt,
            "Byte[]" => SqlDbType.Timestamp,
            "DateOnly" => SqlDbType.Date,
            "DateTime" => SqlDbType.DateTime,
            "DateTimeOffset" => SqlDbType.DateTimeOffset,
            "Decimal" => SqlDbType.Decimal,
            "Double" => SqlDbType.Float,
            "Guid" => SqlDbType.UniqueIdentifier,
            "Int16" => SqlDbType.SmallInt,
            "Int32" => SqlDbType.Int,
            "String" => SqlDbType.VarChar,
            _ => SqlDbType.NChar
        };
    }
}