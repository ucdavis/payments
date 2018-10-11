using System;
using System.Collections.Generic;
using System.Text;
using Payments.Core.Domain;
using TestHelpers.Helpers;
using Xunit;

namespace payments.Tests.DatabaseTests
{
    [Trait("Category", "DatabaseTests")]
    public class MoneyMovementJobRecordTests
    {
        [Fact]
        public void TestAllFieldsInTheDatabaseHaveBeenTested()
        {
            #region Arrange
            var expectedFields = new List<NameAndType>();

            expectedFields.Add(new NameAndType("Id", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Logs", "System.Collections.Generic.IList`1[Payments.Core.Domain.LogMessage]", new List<string>()));
            expectedFields.Add(new NameAndType("Name", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("RanOn", "System.DateTime", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.DisplayAttribute(Name = \"Ran On\")]",
            }));
            expectedFields.Add(new NameAndType("Status", "System.String", new List<string>()));
            #endregion Arrange

            AttributeAndFieldValidation.ValidateFieldsAndAttributes(expectedFields, typeof(MoneyMovementJobRecord));

        }
    }
}
