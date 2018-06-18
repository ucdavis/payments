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
            expectedFields.Add(new NameAndType("Auth_Amount", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Decision", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("OccuredAt", "System.DateTime", new List<string>()));
            expectedFields.Add(new NameAndType("Reason_Code", "System.Int32", new List<string>()));
            expectedFields.Add(new NameAndType("Req_Reference_Number", "System.Int32", new List<string>()));
            expectedFields.Add(new NameAndType("ReturnedResults", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Transaction_Id", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.KeyAttribute()]",
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