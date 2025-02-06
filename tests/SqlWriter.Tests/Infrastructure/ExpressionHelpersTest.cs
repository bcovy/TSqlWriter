using SqlWriter.Infrastructure;
using System.Linq.Expressions;
using SqlWriter.Tests.Fixtures;

namespace SqlWriter.Tests.Infrastructure
{
    public class ExpressionHelpersTest
    {
        #region BelongsToParameter
        [Fact]
        public void Belongs_to_parameter_operand()
        {
            Expression<Func<QueryableMod1, int>> data = d => d.PropertyID;

            Assert.True(data.Body.BelongsToParameter());
        }

        [Fact]
        public void Belongs_to_parameter_nullable_type()
        {
            Expression<Func<QueryableMod1, DateTime?>> data = d => d.PcoeDate;

            Assert.True(data.Body.BelongsToParameter());
        }
        
        [Fact]
        public void Belongs_to_parameter_is_false()
        {
            Expression<Func<QueryableMod1, bool>> data = d => d.PropertyID == 99;

            Assert.False(data.Body.BelongsToParameter());
        }

        #endregion BelongsToParameter

        #region IsNullUnaryOrConstant
        [Theory]
        [InlineData(null, true)]
        [InlineData("hello", false)]
        public void IsNullUnaryOrConstant_should_identify_constant_with_no_value(string value, bool expected)
        {
            QueryableMod1 model = new();
            var left = Expression.Property(Expression.Constant(model), "Address");
            BinaryExpression binaryExpression = Expression.MakeBinary(ExpressionType.Equal, left, Expression.Constant(value));

            bool actual = binaryExpression.Right.IsNullUnaryOrConstant();
  
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData(32, false)]
        public void IsNullUnaryOrConstant_should_identify_unary_with_no_value(int? value, bool expected)
        {
            QueryableMod3 model = new();
            var left = Expression.Property(Expression.Constant(model), "TaskStatus");
            var right = Expression.MakeUnary(ExpressionType.TypeAs, Expression.Constant(value), typeof(int?));
            var expression = Expression.MakeBinary(ExpressionType.Equal, left, right);

            bool actual = expression.Right.IsNullUnaryOrConstant();

            Assert.Equal(expected, actual);
        }

        #endregion IsNullUnaryOrConstant

        #region GetValue MemberExpression

        [Fact]
        public void GetValue_from_memberexpression_property()
        {
            QueryableMod3 model = new() { Address = "hello world" };
            MemberExpression memberExpression = Expression.Property(Expression.Constant(model), "Address");

            object actual = memberExpression.GetValue();

            Assert.Equal("hello world", actual.ToString());
        }

        class Animal
        {
            public string species;
        }

        [Fact]
        public void GetValue_from_memberexpression_field()
        {
            Animal model = new()
            {
                species = "hello world"
            };
            MemberExpression memberExpression = Expression.Field(Expression.Constant(model), "species");

            object actual = memberExpression.GetValue();

            Assert.Equal("hello world", actual.ToString());
        }

        #endregion GetValue MemberExpression

        #region GetValue

        [Fact]
        public void GetValue_from_new_expression()
        {
            Expression<Func<QueryableMod1, object>> expression = a => new { a.Address, a.PropertyID };

            object actual = expression.Body.GetValue();

            Assert.NotNull(actual);
            Assert.Contains("Address", actual.GetType().GetProperties().Select(x => x.Name));
        }

        [Fact]
        public void GetValue_with_anonymous_object()
        {
            Expression<Func<object>> data = () => new { Id = 23 };
            dynamic d = data.Body.GetValue();

            Assert.Equal(23, d.Id);
        }

        [Fact]
        public void GetValue_from_constant_expression()
        {
            Expression<Func<QueryableMod1, int>> expression = a => 2;

            object actual = expression.Body.GetValue();

            Assert.NotNull(actual);
            Assert.Equal(2, actual);
        }

        [Fact]
        public void GetValue_for_property()
        {
            QueryableMod2 data = new() { PropertyID = 2 };
            Expression<Func<QueryableMod1, bool>> expression = a => a.PropertyID == data.PropertyID;

            object actual = ((BinaryExpression)expression.Body).Right.GetValue();

            int result = Assert.IsAssignableFrom<int>(actual);
            Assert.Equal(2, result);
        }

        [Fact]
        public void GetValue_for_nullable_property()
        {
            QueryableMod2 data = new() { YesNo = 2 };
            Expression<Func<QueryableMod2, bool>> expression = a => a.YesNo == data.YesNo;

            object actual = ((BinaryExpression)expression.Body).Right.GetValue();

            int result = Assert.IsAssignableFrom<int>(actual);
            Assert.Equal(2, result);
        }

        [Fact]
        public void GetValue_for_field()
        {
            int propertyID = 2;
            Expression<Func<QueryableMod1, bool>> expression = a => a.PropertyID == propertyID;

            object actual = ((BinaryExpression)expression.Body).Right.GetValue();

            int result = Assert.IsAssignableFrom<int>(actual);
            Assert.Equal(2, result);
        }

        [Fact]
        public void GetValue_from_method_call_expression()
        {
            var hella = new int[] { 1, 2 };
            Expression<Func<QueryableMod1, bool>> expression = a => hella.Contains(1);

            object actual = expression.Body.GetValue();

            Assert.NotNull(actual);
            Assert.Equal(true, actual);
        }

        #endregion GetValue
    }
}