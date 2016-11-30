using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Payments.Models;
using Payments.Tests.Helpers;
using Xunit;


namespace Payments.Tests.TestsModel
{
    public class InvoiceEditViewModelTests
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
            expectedFields.Add(new NameAndType("Title", "System.String", new List<string>
            {
                 "[System.ComponentModel.DataAnnotations.RequiredAttribute()]"
            }));
            expectedFields.Add(new NameAndType("TotalAmount", "System.Decimal", new List<string>
            {
                 "[System.ComponentModel.DataAnnotations.RangeAttribute((Double)0.01, (Double)2147483647, ErrorMessage = \"Must be great than $0.00\")]"
            }));

            #endregion Arrange

            AttributeAndFieldValidation.ValidateFieldsAndAttributes(expectedFields, typeof(InvoiceEditViewModel));

        }
    }
}
