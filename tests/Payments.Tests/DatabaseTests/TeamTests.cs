using System.Collections.Generic;
using Payments.Core.Domain;
using Shouldly;
using TestHelpers.Helpers;
using Xunit;

namespace Payments.Tests.DatabaseTests
{
    [Trait("Category", "DatabaseTests")]
    public class TeamTests
    {
        [Fact]
        public void TestAllFieldsInTheDatabaseHaveBeenTested()
        {
            #region Arrange
            var expectedFields = new List<NameAndType>();
            expectedFields.Add(new NameAndType("Accounts", "System.Collections.Generic.List`1[Payments.Core.Domain.FinancialAccount]", new List<string>()));
            expectedFields.Add(new NameAndType("DefaultAccount", "Payments.Core.Domain.FinancialAccount", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute()]",
            }));
            expectedFields.Add(new NameAndType("Id", "System.Int32", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.KeyAttribute()]",
            }));
            expectedFields.Add(new NameAndType("IsActive", "System.Boolean", new List<string>()));
            expectedFields.Add(new NameAndType("Name", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.DisplayAttribute(Name = \"Team Name\")]",
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)128)]",
            }));
            expectedFields.Add(new NameAndType("Slug", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.DisplayAttribute(Name = \"Team Slug\")]",
                "[System.ComponentModel.DataAnnotations.RegularExpressionAttribute(\"^([a-z0-9]+[a-z0-9\\-]?)+[a-z0-9]$\", ErrorMessage = \"Slug may only contain lowercase alphanumeric characters or single hyphens, and cannot begin or end with a hyphen\")]",
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)40, MinimumLength = 3, ErrorMessage = \"Slug must be between 3 and 40 characters\")]",
            }));

            #endregion Arrange

            AttributeAndFieldValidation.ValidateFieldsAndAttributes(expectedFields, typeof(Team));
        }

        [Fact]
        public void TestAccountsIsInited()
        {
            // Arrange
            var team = new Team();


            // Act


            // Assert		
            team.Accounts.ShouldNotBeNull();
        }
    }
}