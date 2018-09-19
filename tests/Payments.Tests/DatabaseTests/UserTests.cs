using System.Collections.Generic;
using Payments.Core.Domain;
using TestHelpers.Helpers;
using Xunit;

namespace Payments.Tests.DatabaseTests
{
    [Trait("Category", "DatabaseTests")]
    public class UserTests
    {
        [Fact]
        public void TestAllFieldsInTheDatabaseHaveBeenTested()
        {
            #region Arrange
            var expectedFields = new List<NameAndType>();

            expectedFields.Add(new NameAndType("AccessFailedCount", "System.Int32", new List<string>()));
            expectedFields.Add(new NameAndType("CampusKerberos", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.DisplayAttribute(Name = \"Campus Kerberos\")]",
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)50)]",
            }));
            expectedFields.Add(new NameAndType("ConcurrencyStamp", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Email", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.EmailAddressAttribute()]",
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)256)]",
            }));
            expectedFields.Add(new NameAndType("EmailConfirmed", "System.Boolean", new List<string>
            {
                "[Microsoft.AspNetCore.Identity.PersonalDataAttribute()]",
            }));
            expectedFields.Add(new NameAndType("EmailHash", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute()]",
            }));
            expectedFields.Add(new NameAndType("FirstName", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.DisplayAttribute(Name = \"First Name\")]",
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)50)]",
            }));
            expectedFields.Add(new NameAndType("Id", "System.String", new List<string>
            {
                "[Microsoft.AspNetCore.Identity.PersonalDataAttribute()]",
            }));
            expectedFields.Add(new NameAndType("LastName", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.DisplayAttribute(Name = \"Last Name\")]",
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)50)]",
            }));
            expectedFields.Add(new NameAndType("LockoutEnabled", "System.Boolean", new List<string>()));
            expectedFields.Add(new NameAndType("LockoutEnd", "System.Nullable`1[System.DateTimeOffset]", new List<string>()));
            expectedFields.Add(new NameAndType("Name", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.DisplayAttribute(Name = \"Name\")]",
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
                "[System.ComponentModel.DataAnnotations.StringLengthAttribute((Int32)256)]",
            }));
            expectedFields.Add(new NameAndType("NormalizedEmail", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("NormalizedUserName", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("PasswordHash", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("PhoneNumber", "System.String", new List<string>
            {
                "[Microsoft.AspNetCore.Identity.ProtectedPersonalDataAttribute()]",
            }));
            expectedFields.Add(new NameAndType("PhoneNumberConfirmed", "System.Boolean", new List<string>
            {
                "[Microsoft.AspNetCore.Identity.PersonalDataAttribute()]",
            }));
            expectedFields.Add(new NameAndType("SecurityStamp", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("TeamPermissions", "System.Collections.Generic.ICollection`1[Payments.Core.Domain.TeamPermission]", new List<string>()));
            expectedFields.Add(new NameAndType("TwoFactorEnabled", "System.Boolean", new List<string>
            {
                "[Microsoft.AspNetCore.Identity.PersonalDataAttribute()]",
            }));
            expectedFields.Add(new NameAndType("UserName", "System.String", new List<string>
            {
                "[Microsoft.AspNetCore.Identity.ProtectedPersonalDataAttribute()]",
            }));
            #endregion Arrange

            AttributeAndFieldValidation.ValidateFieldsAndAttributes(expectedFields, typeof(User));
        }
    }
}