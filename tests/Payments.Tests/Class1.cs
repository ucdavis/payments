using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    }
}
