using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Payments.Core.Models;
using Payments.Tests.Helpers;
using Xunit;

namespace Payments.Tests.TestsModel
{
    public class InvoiceTests
    {
        [Fact]
        public void TestFieldsHaveExpectedAttributes()
        {
            #region Arrange
            var expectedFields = new List<NameAndType>();
            expectedFields.Add(new NameAndType("ClientEmail", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.EmailAddressAttribute()]",
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)255)]"
            }));
            expectedFields.Add(new NameAndType("ClientName", "System.String", new List<string>
            {
                 "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
                 "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)255)]"
            }));
            expectedFields.Add(new NameAndType("History", "System.Collections.Generic.ICollection`1[Payments.Core.Models.History]", new List<string>()));
            expectedFields.Add(new NameAndType("Id", "System.Int32", new List<string>()));
            expectedFields.Add(new NameAndType("LineItems", "System.Collections.Generic.ICollection`1[Payments.Core.Models.LineItem]", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]"
            }));
            expectedFields.Add(new NameAndType("Scrubbers", "System.Collections.Generic.ICollection`1[Payments.Core.Models.Scrubber]", new List<string>()));
            expectedFields.Add(new NameAndType("Status", "Payments.Core.Models.InvoiceStatus", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]"
            }));
            expectedFields.Add(new NameAndType("Title", "System.String", new List<string>
            {
                 "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)128)]"
            }));
            expectedFields.Add(new NameAndType("TotalAmount", "System.Decimal", new List<string>
            {
                 "[System.ComponentModel.DataAnnotations.RangeAttribute((Double)0.01, (Double)2147483647, ErrorMessage = \"Must be at least $0.01\")]",
                 "[System.ComponentModel.DataAnnotations.RequiredAttribute()]"
            }));
            #endregion Arrange

            AttributeAndFieldValidation.ValidateFieldsAndAttributes(expectedFields, typeof(Invoice));

        }
    }
}
