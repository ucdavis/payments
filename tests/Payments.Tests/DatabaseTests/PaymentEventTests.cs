using System.Collections.Generic;
using Payments.Core.Domain;
using Shouldly;
using TestHelpers.Helpers;
using Xunit;

namespace Payments.Tests.DatabaseTests
{
    [Trait("Category", "DatabaseTests")]
    public class PaymentEventTests
    {
        [Fact]
        public void TestAllFieldsInTheDatabaseHaveBeenTested()
        {
            #region Arrange
            var expectedFields = new List<NameAndType>();
            expectedFields.Add(new NameAndType("Amount", "System.Decimal", new List<string>()));
            expectedFields.Add(new NameAndType("BillingCity", "System.String", new List<string>{
                "[System.ComponentModel.DataAnnotations.MaxLengthAttribute((Int32)50)]",
                "[System.ComponentModel.DisplayNameAttribute(\"Billing City\")]",
            }));
            expectedFields.Add(new NameAndType("BillingCountry", "System.String", new List<string>{
                "[System.ComponentModel.DataAnnotations.MaxLengthAttribute((Int32)2)]",
                "[System.ComponentModel.DisplayNameAttribute(\"Billing Country\")]",
            }));
            expectedFields.Add(new NameAndType("BillingEmail", "System.String", new List<string>{
                "[System.ComponentModel.DataAnnotations.MaxLengthAttribute((Int32)255)]",
                "[System.ComponentModel.DisplayNameAttribute(\"Billing Email\")]",
            }));
            expectedFields.Add(new NameAndType("BillingFirstName", "System.String", new List<string>{
                "[System.ComponentModel.DataAnnotations.MaxLengthAttribute((Int32)60)]",
                "[System.ComponentModel.DisplayNameAttribute(\"Billing First Name\")]",
            }));
            expectedFields.Add(new NameAndType("BillingLastName", "System.String", new List<string>{
                "[System.ComponentModel.DataAnnotations.MaxLengthAttribute((Int32)60)]",
                "[System.ComponentModel.DisplayNameAttribute(\"Billing Last Name\")]",
            }));
            expectedFields.Add(new NameAndType("BillingPhone", "System.String", new List<string>{
                "[System.ComponentModel.DataAnnotations.MaxLengthAttribute((Int32)15)]",
                "[System.ComponentModel.DisplayNameAttribute(\"Billing Phone\")]",
            }));
            expectedFields.Add(new NameAndType("BillingPostalCode", "System.String", new List<string>{
                "[System.ComponentModel.DataAnnotations.MaxLengthAttribute((Int32)10)]",
                "[System.ComponentModel.DisplayNameAttribute(\"Billing Postal Code\")]",
            }));
            expectedFields.Add(new NameAndType("BillingState", "System.String", new List<string>{
                "[System.ComponentModel.DataAnnotations.MaxLengthAttribute((Int32)2)]",
                "[System.ComponentModel.DisplayNameAttribute(\"Billing State\")]",
            }));
            expectedFields.Add(new NameAndType("BillingStreet1", "System.String", new List<string>{
                "[System.ComponentModel.DataAnnotations.MaxLengthAttribute((Int32)60)]",
                "[System.ComponentModel.DisplayNameAttribute(\"Billing Street\")]",
            }));
            expectedFields.Add(new NameAndType("BillingStreet2", "System.String", new List<string>{
                "[System.ComponentModel.DataAnnotations.MaxLengthAttribute((Int32)60)]",
                "[System.ComponentModel.DisplayNameAttribute(\"Billing Street 2\")]",
            }));
            expectedFields.Add(new NameAndType("Decision", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Id", "System.Int32", new List<string>{
                "[System.ComponentModel.DataAnnotations.KeyAttribute()]",
            }));
            expectedFields.Add(new NameAndType("Invoice", "Payments.Core.Domain.Invoice", new List<string>()));
            expectedFields.Add(new NameAndType("OccuredAt", "System.DateTime", new List<string>()));
            expectedFields.Add(new NameAndType("Processor", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("ProcessorId", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("ReturnedResults", "System.String", new List<string>{
                "[Newtonsoft.Json.JsonIgnoreAttribute()]",
            }));

            
            #endregion Arrange

            AttributeAndFieldValidation.ValidateFieldsAndAttributes(expectedFields, typeof(PaymentEvent));
        }

        [Fact]
        public void TestOccuredAtIsDefaulted()
        {
            // Arrange
            var paymentEvent = new PaymentEvent();


            // Act


            // Assert		
            paymentEvent.ShouldNotBeNull();
        }
    }
}