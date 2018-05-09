using System;
using System.Collections.Generic;
using System.Text;
using Payments.Core.Domain;
using TestHelpers.Helpers;
using Xunit;

namespace Payments.Tests.DatabaseTests
{
    [Trait("Category", "DatabaseTests")]
    public class InvoiceTests
    {
        [Fact]
        public void TestAllFieldsInTheDatabaseHaveBeenTested()
        {
            #region Arrange
            var expectedFields = new List<NameAndType>();

            expectedFields.Add(new NameAndType("Account", "Payments.Core.Domain.FinancialAccount", new List<string>()));
            expectedFields.Add(new NameAndType("Creator", "Payments.Core.Domain.User", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
            }));
            expectedFields.Add(new NameAndType("CustomerAddress", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("CustomerEmail", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.EmailAddressAttribute()]",
            }));
            expectedFields.Add(new NameAndType("CustomerName", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Discount", "System.Decimal", new List<string>()));
            expectedFields.Add(new NameAndType("Id", "System.Int32", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.KeyAttribute()]",
            }));
            expectedFields.Add(new NameAndType("Items", "System.Collections.Generic.List`1[Payments.Core.Domain.LineItem]", new List<string>()));
            expectedFields.Add(new NameAndType("Memo", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Payment", "Payments.Core.Domain.PaymentEvent", new List<string>()));
            expectedFields.Add(new NameAndType("Status", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Subtotal", "System.Decimal", new List<string>()));
            expectedFields.Add(new NameAndType("TaxAmount", "System.Decimal", new List<string>()));
            expectedFields.Add(new NameAndType("TaxPercent", "System.Decimal", new List<string>()));
            expectedFields.Add(new NameAndType("Team", "Payments.Core.Domain.Team", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
            }));
            expectedFields.Add(new NameAndType("Total", "System.Decimal", new List<string>()));

            #endregion Arrange

            AttributeAndFieldValidation.ValidateFieldsAndAttributes(expectedFields, typeof(Invoice));

        }
    }
}
