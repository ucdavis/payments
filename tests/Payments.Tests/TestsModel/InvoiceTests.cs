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
            expectedFields.Add(new NameAndType("Id", "System.Int32", new List<string>()));
            expectedFields.Add(new NameAndType("Title", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("TotalAmount", "System.Decimal", new List<string>()));

            #endregion Arrange

            AttributeAndFieldValidation.ValidateFieldsAndAttributes(expectedFields, typeof(Invoice));

        }
    }
}
