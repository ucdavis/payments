using Payments.Core.Domain;
using System.Collections.Generic;
using TestHelpers.Helpers;
using Xunit;

namespace payments.Tests.DatabaseTests
{
    [Trait("Category", "DatabaseTests")]
    public class LogMessageTests
    {
        [Fact]
        public void TestAllFieldsInTheDatabaseHaveBeenTested()
        {
            #region Arrange
            var expectedFields = new List<NameAndType>();

            expectedFields.Add(new NameAndType("CorrelationId", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Exception", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Id", "System.Int32", new List<string>()));
            expectedFields.Add(new NameAndType("JobId", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("JobName", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Level", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("LogEvent", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Message", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("MessageTemplate", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Properties", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Source", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("TimeStamp", "System.DateTimeOffset", new List<string>()));
            #endregion Arrange

            AttributeAndFieldValidation.ValidateFieldsAndAttributes(expectedFields, typeof(LogMessage));

        }
    }
}
