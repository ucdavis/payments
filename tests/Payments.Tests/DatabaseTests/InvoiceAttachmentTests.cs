using System;
using System.Collections.Generic;
using System.Text;
using Payments.Core.Domain;
using TestHelpers.Helpers;
using Xunit;

namespace payments.Tests.DatabaseTests
{
    [Trait("Category", "DatabaseTests")]
    public class InvoiceAttachmentTests
    {
        [Fact]
        public void TestAllFieldsInTheDatabaseHaveBeenTested()
        {
            #region Arrange
            var expectedFields = new List<NameAndType>();

            expectedFields.Add(new NameAndType("ContentType", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("FileName", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Id", "System.Int32", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.KeyAttribute()]",
            }));
            expectedFields.Add(new NameAndType("Identifier", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Invoice", "Payments.Core.Domain.Invoice", new List<string>
            {
                "[Newtonsoft.Json.JsonIgnoreAttribute()]",
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
            }));
            expectedFields.Add(new NameAndType("Size", "System.Int64", new List<string>()));
 
            #endregion Arrange

            AttributeAndFieldValidation.ValidateFieldsAndAttributes(expectedFields, typeof(InvoiceAttachment));

        }
    }
}
