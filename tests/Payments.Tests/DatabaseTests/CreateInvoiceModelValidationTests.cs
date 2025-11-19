using Payments.Core.Models.Invoice;
using Payments.Core.Domain;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace Payments.Tests.DatabaseTests
{
    [Trait("Category", "DatabaseTests")]
    public class CreateInvoiceModelValidationTests
    {
        [Fact]
        public void CreateInvoiceModel_Type_ValidValues_ShouldPass()
        {
            // Arrange
            var model = new CreateInvoiceModel
            {
                Type = "CC"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.DoesNotContain(validationResults, v => v.MemberNames.Contains("Type"));
        }

        [Fact]
        public void CreateInvoiceModel_Type_ValidRechargeValue_ShouldPass()
        {
            // Arrange
            var model = new CreateInvoiceModel
            {
                Type = "Recharge"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.DoesNotContain(validationResults, v => v.MemberNames.Contains("Type"));
        }

        [Fact]
        public void CreateInvoiceModel_Type_UsesInvoiceTypeConstants_ShouldPass()
        {
            // Arrange & Act - Test using the actual constants from Invoice.InvoiceTypes
            var ccModel = new CreateInvoiceModel { Type = Invoice.InvoiceTypes.CreditCard };
            var rechargeModel = new CreateInvoiceModel { Type = Invoice.InvoiceTypes.Recharge };

            var ccResults = ValidateModel(ccModel);
            var rechargeResults = ValidateModel(rechargeModel);

            // Assert
            Assert.DoesNotContain(ccResults, v => v.MemberNames.Contains("Type"));
            Assert.DoesNotContain(rechargeResults, v => v.MemberNames.Contains("Type"));
        }

        [Fact]
        public void CreateInvoiceModel_Type_InvalidValue_ShouldFail()
        {
            // Arrange
            var model = new CreateInvoiceModel
            {
                Type = "InvalidType"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Contains(validationResults, v =>
                v.MemberNames.Contains("Type") &&
                v.ErrorMessage == "The Type field must be either 'CC' or 'Recharge'.");
        }

        [Fact]
        public void CreateInvoiceModel_Type_NullValue_ShouldFail()
        {
            // Arrange
            var model = new CreateInvoiceModel
            {
                Type = null
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Type"));
        }

        [Fact]
        public void CreateInvoiceModel_Type_EmptyValue_ShouldFail()
        {
            // Arrange
            var model = new CreateInvoiceModel
            {
                Type = ""
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Type"));
        }

        private static IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }
    }
}