using System.Collections.Generic;
using Payments.Core.Domain;
using Shouldly;
using TestHelpers.Helpers;
using Xunit;

namespace Payments.Tests.DatabaseTests
{
    [Trait("Category", "DatabaseTests")]
    public class TeamRoleTests
    {
        [Fact]
        public void TestAllFieldsInTheDatabaseHaveBeenTested()
        {
            #region Arrange
            var expectedFields = new List<NameAndType>();

            expectedFields.Add(new NameAndType("Id", "System.Int32", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.KeyAttribute()]",
            }));
            expectedFields.Add(new NameAndType("Name", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.DisplayAttribute(Name = \"Role Name\")]",
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)50)]"
            }));

            #endregion Arrange

            AttributeAndFieldValidation.ValidateFieldsAndAttributes(expectedFields, typeof(TeamRole));
        }

        [Fact]
        public void TestStatusCodesHaveExpectedValues()
        {
            var scType = typeof(TeamRole.Codes);
            var props = scType.GetFields();
            props.Length.ShouldBe(4);

            TeamRole.Codes.Editor.ShouldBe("Editor");
            TeamRole.Codes.Admin.ShouldBe("Admin");
            TeamRole.Codes.ReportUser.ShouldBe("ReportUser");
            TeamRole.Codes.FinanceOfficer.ShouldBe("FinanceOfficer");
        }
    }    
}