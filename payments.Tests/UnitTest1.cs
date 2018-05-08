using System;
using System.Linq;
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
    }
}
