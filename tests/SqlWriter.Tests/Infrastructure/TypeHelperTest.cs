using SqlWriter.Infrastructure;

namespace SqlWriter.Tests.Infrastructure;

public class TypeHelperTest
{
    public static TheoryData<object> NumericData => new()
    {
        { 1 }, { 2.5M }, { -32768 }, { 2147483647 }, { 9223372036854775807 }, { 1.1 }, { 134.45E-2f }, { (int?)100 }
    };

    [Theory]
    [MemberData(nameof(NumericData))]
    public void IsNumeric_returns_true_when_object_value_is_numeric(object value)
    {
        bool actual = TypeHelper.IsNumeric(value);

        Assert.True(actual);
    }

    public static TheoryData<object, bool> DateTimeData => new()
    {
        { 1, false }, { DateTime.Now, true }, { new DateOnly(2024, 4, 1), false }, { (DateTime?)DateTime.Now, true },
    };

    [Theory]
    [MemberData(nameof(DateTimeData))]
    public void IsDateTime_identifies_datetime_types(object value, bool expected)
    {
        bool actual = TypeHelper.IsDateTime(value);

        Assert.Equal(expected, actual);
    }
}
