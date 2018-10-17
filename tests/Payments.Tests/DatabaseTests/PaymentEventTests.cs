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