using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Payments.Tests
{
    [Trait("Category", "Sample")]
    public class Class1
    {
        
        [Fact]
        public void Test1()
        {
            Assert.Equal(4, 2 + 2);
        }

        [Fact(Skip = "Test skipped because it is a test of the fail")]
        public void BadMath()
        {
            Assert.Equal(4, 1+1);
        }

        [Theory]
        [InlineData(3, true)]
        [InlineData(5, true)]
        [InlineData(6, false)]
        public void MyFirstTheory(int value, bool expected)
        {
            IsOdd(value).ShouldBe(expected);
        }

        bool IsOdd(int value)
        {
            return value % 2 == 1;
        }


        [Fact]
        public void TestExceptionExample()
        {
            // Arrange

            // Act
            var ex = Assert.Throws<NotImplementedException>(() => ExceptionTest());

            // Assert
            ex.ShouldBeOfType<NotImplementedException>();
            ex.Message.ShouldBe("Party on dude!");
        }

        private void ExceptionTest()
        {
            throw new NotImplementedException("Party on dude!");
        }
    }
}


