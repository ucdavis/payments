using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Payments.Tests
{
    public class Class1
    {
        [Fact]
        public void Test1()
        {
            Assert.Equal(4, 2 + 2);
        }

        [Fact]
        public void BadMath()
        {
            Assert.Equal(4, 1+1);
        }
    }
}
