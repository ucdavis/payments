using System;
using System.Globalization;
using System.Linq;
using Castle.Components.DictionaryAdapter;
using Payments.Core.Extensions;
using Serilog;
using Serilog.Sinks.TestCorrelator;
using Shouldly;
using Xunit;

namespace payments.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var xxx = "Test";
            xxx.ShouldBe("Test");
        }

        [Fact]
        public void TestDecimal1()
        {
            decimal xxx = 0.0m;
            if (decimal.TryParse("1.50", NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal amount))
            {
                xxx = amount;
            }
            xxx.ShouldBe(1.50m);
        }

        [Fact]
        public void TestDecimal2()
        {
            decimal xxx = 0.0m;
            if (decimal.TryParse("1.50", out decimal amount))
            {
                xxx = amount;
            }
            xxx.ShouldBe(1.50m);
        }

        [Fact]
        public void TestSampleLog()
        {
            using (TestCorrelator.CreateContext())
            {
                Log.Logger = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();

                Log.Information("My log message!");

                var logInfo = TestCorrelator.GetLogEventsFromCurrentContext();
                logInfo.Count().ShouldBe(1);
                logInfo.ElementAt(0).MessageTemplate.Text.ShouldBe("My log message!");
                
            }
        }

        [Theory]
        [InlineData("Abc 123-.' xxx", "Abc 123-.' xxx")]
        [InlineData("Abc;cbA", "AbccbA")]
        [InlineData("Abc!@#$%^&*()++cbA", "AbccbA")]
        [InlineData(null, null)]
        [InlineData(" ", " ")]
        public void TestRegex(string value, string result)
        {
            value.SafeRegexRemove().ShouldBe(result);
        }
    }
}
