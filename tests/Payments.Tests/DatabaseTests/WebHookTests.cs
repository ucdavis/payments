using System;
using System.Collections.Generic;
using System.Text;
using Payments.Core.Domain;
using TestHelpers.Helpers;
using Xunit;

namespace payments.Tests.DatabaseTests
{
    [Trait("Category", "DatabaseTests")]
    public class WebHookTests
    {
        [Fact]
        public void TestAllFieldsInTheDatabaseHaveBeenTested()
        {
            #region Arrange
            var expectedFields = new List<NameAndType>();

            expectedFields.Add(new NameAndType("ContentType", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Id", "System.Int32", new List<string>()));
            expectedFields.Add(new NameAndType("IsActive", "System.Boolean", new List<string>
            {
                "[System.ComponentModel.DisplayNameAttribute(\"Enabled\")]",
            }));
            expectedFields.Add(new NameAndType("Team", "Payments.Core.Domain.Team", new List<string>
            {
                "[Newtonsoft.Json.JsonIgnoreAttribute()]",
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
            }));
            expectedFields.Add(new NameAndType("TeamId", "System.Int32", new List<string>()));
            expectedFields.Add(new NameAndType("TriggerOnPaid", "System.Boolean", new List<string>
            {
                "[System.ComponentModel.DisplayNameAttribute(\"Trigger on Paid\")]",
            }));
            expectedFields.Add(new NameAndType("Url", "System.String", new List<string>
            {
                "[System.ComponentModel.DisplayNameAttribute(\"Payload URL\")]",
            }));

            #endregion Arrange

            AttributeAndFieldValidation.ValidateFieldsAndAttributes(expectedFields, typeof(WebHook));

        }
    }
}
