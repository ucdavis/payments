using System;
using System.Collections.Generic;
using System.Text;
using Payments.Core.Domain;
using TestHelpers.Helpers;
using Xunit;

namespace Payments.Tests.DatabaseTests
{
    [Trait("Category", "DatabaseTests")]
    public class HistoryTests
    {
        [Fact]
        public void TestAllFieldsInTheDatabaseHaveBeenTested()
        {
            #region Arrange
            var expectedFields = new List<NameAndType>();
            expectedFields.Add(new NameAndType("ActionDateTime", "System.DateTime", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
            }));
            expectedFields.Add(new NameAndType("Actor", "Payments.Core.Domain.User", new List<string>()));
            expectedFields.Add(new NameAndType("ActorId", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Data", "System.String", new List<string>()));

            expectedFields.Add(new NameAndType("Id", "System.Int32", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.KeyAttribute()]",
            }));
            expectedFields.Add(new NameAndType("InvoiceId", "System.Int32", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
            }));
            expectedFields.Add(new NameAndType("Type", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
            }));
                        
            #endregion Arrange

            AttributeAndFieldValidation.ValidateFieldsAndAttributes(expectedFields, typeof(History));

        }
    }
}
