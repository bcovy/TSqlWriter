using SqlWriter.Infrastructure;
using System.Data;
using SqlWriter.Tests.Fixtures;

namespace SqlWriter.Tests.Infrastructure
{
    public class TranslateDbTypeTest
    {
        [Fact]
        public void Should_translate_int_type_from_property()
        {
            QueryableMod1 model = new();

            var actual = model.PropertyID.TranslateSqlDbType();

            Assert.Equal(SqlDbType.Int, actual);
        }

        [Fact]
        public void Should_translate_datetime_nullable_type_from_property()
        {
            QueryableMod1 model = new();

            var actual = model.PcoeDate.TranslateSqlDbType();

            Assert.Equal(SqlDbType.DateTime, actual);
        }

        [Fact]
        public void Should_translate_datetime_nullable_type_from_field_value()
        {
            DateTime? value = null;

            var actual = value.TranslateSqlDbType();

            Assert.Equal(SqlDbType.DateTime, actual);
        }

        [Fact]
        public void Should_translate_datetime_type_from_field_value()
        {
            DateTime value = DateTime.Now;

            var actual = value.TranslateSqlDbType();

            Assert.Equal(SqlDbType.DateTime, actual);
        }

        [Fact]
        public void Should_translate_dateonly_nullable_type_from_field_value()
        {
            DateOnly? value = null;

            var actual = value.TranslateSqlDbType();

            Assert.Equal(SqlDbType.Date, actual);
        }

        [Fact]
        public void Should_translate_dateonly_type_from_field_value()
        {
            DateOnly value = DateOnly.FromDateTime(DateTime.Now);

            var actual = value.TranslateSqlDbType();

            Assert.Equal(SqlDbType.Date, actual);
        }

        [Fact]
        public void Should_identify_underlying_decimal_type_from_object()
        {
            object value = 1.25M;

            var actual = value.TranslateSqlDbType();

            Assert.Equal(SqlDbType.Decimal, actual);
        }

        [Fact]
        public void Should_identify_underlying_string_type_from_object()
        {
            object value = "hello world!";

            var actual = value.TranslateSqlDbType();

            Assert.Equal(SqlDbType.VarChar, actual);
        }

        [Fact]
        public void Should_translate_datetime_nullable_type_from_propertyinfo()
        {
            QueryableMod1 model = new();
            var value = model.GetType().GetProperty("PcoeDate");

            var actual = value.TranslateSqlDbType();

            Assert.Equal(SqlDbType.DateTime, actual);
        }

        [Fact]
        public void Should_translate_int_type_from_propertyinfo()
        {
            QueryableMod1 model = new();
            var value = model.GetType().GetProperty("PropertyID");

            var actual = value.TranslateSqlDbType();

            Assert.Equal(SqlDbType.Int, actual);
        }

        public DateTime? CloseDate { get; set; }
        public int EventID { get; set; }

        [Fact]
        public void Should_translate_datetime_nullable_type_from_field_property()
        {
            var actual = CloseDate.TranslateSqlDbType();

            Assert.Equal(SqlDbType.DateTime, actual);
        }

        [Fact]
        public void Should_translate_int_type_from_field_property()
        {
            var actual = EventID.TranslateSqlDbType();

            Assert.Equal(SqlDbType.Int, actual);
        }
    }
}