using Payments.Core.Domain;
using System.Collections.Generic;
using TestHelpers.Helpers;
using Xunit;

namespace Payments.Tests.DatabaseTests
{
    [Trait("Category", "DatabaseTests")]
    public class FinancialAccountTests
    {
        [Fact]
        public void TestAllFieldsInTheDatabaseHaveBeenTested()
        {
            #region Arrange
            var expectedFields = new List<NameAndType>();

            expectedFields.Add(new NameAndType("Account", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.RegularExpressionAttribute(\"[A-Z0-9]*\")]",
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)7)]",  
            }));
            expectedFields.Add(new NameAndType("Chart", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)1)]",
            }));
            expectedFields.Add(new NameAndType("Description", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("FinancialSegmentString", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)128)]",
                "[System.ComponentModel.DisplayNameAttribute(\"Financial Segment String\")]",
            }));
            expectedFields.Add(new NameAndType("Id", "System.Int32", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.KeyAttribute()]",
            }));
            expectedFields.Add(new NameAndType("IsActive", "System.Boolean", new List<string>
            {
                "[System.ComponentModel.DisplayNameAttribute(\"Active\")]",
            }));
            expectedFields.Add(new NameAndType("IsDefault", "System.Boolean", new List<string>
            {
                "[System.ComponentModel.DisplayNameAttribute(\"Default\")]",
            }));
            expectedFields.Add(new NameAndType("Name", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)128)]",
            }));
            expectedFields.Add(new NameAndType("Project", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.DisplayFormatAttribute(NullDisplayText = \"---------\")]",
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)9)]",
            }));
            expectedFields.Add(new NameAndType("SubAccount", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.DisplayFormatAttribute(NullDisplayText = \"-----\")]",
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)5)]",
            }));
            expectedFields.Add(new NameAndType("Team", "Payments.Core.Domain.Team", new List<string>
            {
                "[Newtonsoft.Json.JsonIgnoreAttribute()]",
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
            }));
            expectedFields.Add(new NameAndType("TeamId", "System.Int32", new List<string>()));            
            #endregion Arrange

            AttributeAndFieldValidation.ValidateFieldsAndAttributes(expectedFields, typeof(FinancialAccount));

        }
    }
}
