using System.Collections.Generic;
using Payments.Core.Domain;
using TestHelpers.Helpers;
using Xunit;

namespace Payments.Tests.DatabaseTests
{
    [Trait("Category", "DatabaseTests")]
    public class LineItemTests
    {
        [Fact]
        public void TestAllFieldsInTheDatabaseHaveBeenTested()
        {
            #region Arrange
            var expectedFields = new List<NameAndType>();

            expectedFields.Add(new NameAndType("Amount", "System.Decimal", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.DisplayFormatAttribute(DataFormatString = \"{0:C}\")]",
                "[System.ComponentModel.DataAnnotations.RangeAttribute((Int32)0, (Int32)1000000)]",                
            }));
            expectedFields.Add(new NameAndType("Description", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Id", "System.Int32", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.KeyAttribute()]",
            }));
            expectedFields.Add(new NameAndType("Quantity", "System.Decimal", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.RangeAttribute((Int32)0, (Int32)1000000)]",
            }));

            expectedFields.Add(new NameAndType("TaxExempt", "System.Boolean", new List<string>
            {
            }));
            expectedFields.Add(new NameAndType("Total", "System.Decimal", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.DisplayFormatAttribute(DataFormatString = \"{0:C}\")]",
                "[System.ComponentModel.DataAnnotations.RangeAttribute((Int32)0, (Int32)1000000)]",
            }));
            #endregion Arrange

            AttributeAndFieldValidation.ValidateFieldsAndAttributes(expectedFields, typeof(LineItem));
        }
    }
}