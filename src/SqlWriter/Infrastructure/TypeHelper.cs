namespace SqlWriter.Infrastructure;

public static class TypeHelper
{
    private static readonly HashSet<Type> _numericTypes =
    [
        typeof(int),  typeof(double),  typeof(decimal),
        typeof(long), typeof(short),   typeof(sbyte),
        typeof(byte), typeof(ulong),   typeof(ushort),
        typeof(uint), typeof(float)    
    ];

    public static bool IsNumeric(object? value)
    {
        if (value == null)
            return false;
        
        var type = value.GetType();

        return _numericTypes.Contains(Nullable.GetUnderlyingType(type) ?? type);
    }

    public static bool IsNumericType(Type? type)
    {
        return type != null && _numericTypes.Contains(Nullable.GetUnderlyingType(type) ?? type);
    }

    public static bool IsDateTime(object? value)
    {
        if (value is null)
            return false;
        
        var type = value.GetType();

        return Type.GetTypeCode(type) == TypeCode.DateTime;
    }
}
