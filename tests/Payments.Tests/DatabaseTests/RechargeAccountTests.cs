using Payments.Core.Domain;
using System.Collections.Generic;
using TestHelpers.Helpers;
using Xunit;

namespace Payments.Tests.DatabaseTests
{
    [Trait("Category", "DatabaseTests")]
    public class RechargeAccountTests
    {
        [Fact]
        public void TestAllFieldsInTheDatabaseHaveBeenTested()
        {
            #region Arrange
            var expectedFields = new List<NameAndType>();

            expectedFields.Add(new NameAndType("Amount", "System.Decimal", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.RangeAttribute((Double)0.01, (Double)1000000)]",
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
            }));
            expectedFields.Add(new NameAndType("ApprovedByKerb", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)20)]",
            }));
            expectedFields.Add(new NameAndType("ApprovedByName", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)128)]",
            }));
            expectedFields.Add(new NameAndType("Direction", "Payments.Core.Domain.RechargeAccount+CreditDebit", new List<string>()));
            expectedFields.Add(new NameAndType("EnteredByKerb", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)20)]",
            }));
            expectedFields.Add(new NameAndType("EnteredByName", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)128)]",
            }));
            expectedFields.Add(new NameAndType("FinancialSegmentString", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)128)]",
            }));
            expectedFields.Add(new NameAndType("Id", "System.Int32", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.KeyAttribute()]",
            }));
            expectedFields.Add(new NameAndType("Invoice", "Payments.Core.Domain.Invoice", new List<string>
            {
                "[Newtonsoft.Json.JsonIgnoreAttribute()]",
            }));
            expectedFields.Add(new NameAndType("InvoiceId", "System.Int32", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
            }));
            expectedFields.Add(new NameAndType("Notes", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)400)]",
            }));
            expectedFields.Add(new NameAndType("Percentage", "System.Decimal", new List<string>()));
            #endregion Arrange

            AttributeAndFieldValidation.ValidateFieldsAndAttributes(expectedFields, typeof(RechargeAccount));

        }
    }
}
