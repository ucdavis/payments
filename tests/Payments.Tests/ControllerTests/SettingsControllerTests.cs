using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Models.Configuration;
using Payments.Core.Services;
using Payments.Mvc.Controllers;
using Payments.Mvc.Identity;
using Payments.Mvc.Models.Roles;
using Payments.Mvc.Models.TeamViewModels;
using Xunit;

namespace payments.Tests.ControllerTests
{
    [Trait("Category", "ControllerTests")]
    public class SettingsControllerTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IAggieEnterpriseService> _aggieEnterpriseService;

        public SettingsControllerTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;
            _dbContext = new ApplicationDbContext(options);
            _dbContext.Database.EnsureCreated();
            _dbContext.Teams.Add(new Team
            {
                Id = 1,
                Name = "Alpha Team",
                Slug = "alpha",
                ContactName = "Original Contact",
                ContactEmail = "original@example.com",
                ContactPhoneNumber = "530-555-0100",
                UseCustomAccountValidation = true,
                RechargeDebitNaturalAccount = "123456",
            });
            _dbContext.SaveChanges();
            _dbContext.ChangeTracker.Clear();
            _aggieEnterpriseService = new Mock<IAggieEnterpriseService>(MockBehavior.Strict);
        }

        [Fact]
        public void RechargeDebitNaturalAccountIsDisabledByDefault()
        {
            Assert.False(new FinanceSettings().EnableRechargeDebitNaturalAccount);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task EditLoadsTheStoredCustomAccountSettingsAndConfiguredVisibility(bool featureEnabled)
        {
            var controller = CreateController(enableRechargeDebitNaturalAccount: featureEnabled);

            var result = await controller.Edit();

            var model = Assert.IsType<EditTeamViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.True(model.UseCustomAccountValidation);
            Assert.Equal("123456", model.RechargeDebitNaturalAccount);
            Assert.Equal(featureEnabled, model.ShowRechargeDebitNaturalAccount);
            _aggieEnterpriseService.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task DetailsLoadsTheConfiguredAccountVisibility(bool featureEnabled)
        {
            var userManager = new Mock<ApplicationUserManager>(
                Mock.Of<IUserStore<User>>(), Options.Create(new IdentityOptions()),
                null, null, null, null, null, null, null);
            userManager.Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new User());
            var controller = CreateController(
                enableRechargeDebitNaturalAccount: featureEnabled, userManager: userManager.Object);

            var result = await controller.Index();

            var model = Assert.IsType<TeamDetailsModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(featureEnabled, model.ShowRechargeDebitNaturalAccount);
            Assert.Equal("123456", model.RechargeDebitNaturalAccount);
            _aggieEnterpriseService.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(true, null)]
        [InlineData(true, "654321")]
        [InlineData(true, "invalid-account")]
        [InlineData(false, null)]
        [InlineData(false, "invalid-account")]
        public async Task DisabledFeaturePreservesTheAccountAndIgnoresSubmittedValuesAndErrors(
            bool useCustomAccountValidation, string submittedAccount)
        {
            var controller = CreateController(enableRechargeDebitNaturalAccount: false);
            var model = CreateModel();
            model.UseCustomAccountValidation = useCustomAccountValidation;
            model.RechargeDebitNaturalAccount = submittedAccount;
            model.ShowRechargeDebitNaturalAccount = true;
            ValidateModel(controller, model);
            controller.ModelState.AddModelError(nameof(model.RechargeDebitNaturalAccount), "Submitted account error.");

            var result = await controller.Edit(model);

            Assert.IsType<RedirectToActionResult>(result);
            var stored = await ReadTeam();
            Assert.Equal(model.Name, stored.Name);
            Assert.Equal(useCustomAccountValidation, stored.UseCustomAccountValidation);
            Assert.Equal("123456", stored.RechargeDebitNaturalAccount);
            Assert.False(model.ShowRechargeDebitNaturalAccount);
            Assert.False(controller.ModelState.ContainsKey(nameof(model.RechargeDebitNaturalAccount)));
            _aggieEnterpriseService.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task InvalidEditUsesConfiguredVisibilityInsteadOfThePostedValue(bool featureEnabled)
        {
            var controller = CreateController(enableRechargeDebitNaturalAccount: featureEnabled);
            var model = CreateModel();
            model.ShowRechargeDebitNaturalAccount = !featureEnabled;
            controller.ModelState.AddModelError(nameof(model.Name), "Team name is required.");

            var result = await controller.Edit(model);

            Assert.Same(model, Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(featureEnabled, model.ShowRechargeDebitNaturalAccount);
            Assert.NotEmpty(controller.ModelState[nameof(model.Name)].Errors);
            await AssertOriginalTeamIsUnchanged();
            _aggieEnterpriseService.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("654321")]
        [InlineData("12X456")]
        public async Task SystemUserCanSaveAnAccountValidatedByAggieEnterprise(string account)
        {
            var controller = CreateController();
            var model = CreateModel();
            model.RechargeDebitNaturalAccount = account;
            ValidateModel(controller, model);
            _aggieEnterpriseService.Setup(service => service.IsNaturalAccountValid(account))
                .ReturnsAsync(true);

            var result = await controller.Edit(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(SettingsController.Index), redirect.ActionName);
            var stored = await ReadTeam();
            Assert.Equal(model.Name, stored.Name);
            Assert.True(stored.UseCustomAccountValidation);
            Assert.Equal(account, stored.RechargeDebitNaturalAccount);
            _aggieEnterpriseService.Verify(service => service.IsNaturalAccountValid(account), Times.Once);
            _aggieEnterpriseService.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SystemUserCanEnableCustomValidationWithoutAnOverride(string account)
        {
            var team = await _dbContext.Teams.SingleAsync();
            team.UseCustomAccountValidation = false;
            team.RechargeDebitNaturalAccount = null;
            await _dbContext.SaveChangesAsync();
            _dbContext.ChangeTracker.Clear();
            var controller = CreateController();
            var model = CreateModel();
            model.RechargeDebitNaturalAccount = account;
            ValidateModel(controller, model);

            var result = await controller.Edit(model);

            Assert.IsType<RedirectToActionResult>(result);
            var stored = await ReadTeam();
            Assert.True(stored.UseCustomAccountValidation);
            Assert.True(string.IsNullOrEmpty(stored.RechargeDebitNaturalAccount));
            _aggieEnterpriseService.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("654321")]
        [InlineData("invalid-account")]
        public async Task DisablingCustomValidationClearsTheOverrideAndIgnoresItsValidationErrors(string account)
        {
            var controller = CreateController();
            var model = CreateModel();
            model.UseCustomAccountValidation = false;
            model.RechargeDebitNaturalAccount = account;
            ValidateModel(controller, model);

            var result = await controller.Edit(model);

            Assert.IsType<RedirectToActionResult>(result);
            var stored = await ReadTeam();
            Assert.Equal(model.Name, stored.Name);
            Assert.False(stored.UseCustomAccountValidation);
            Assert.Null(stored.RechargeDebitNaturalAccount);
            _aggieEnterpriseService.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(true, "123456", false)]
        [InlineData(false, null, true)]
        public async Task TeamUserCannotOverpostCustomAccountSettingsButCanSaveOtherEdits(
            bool storedFlag, string storedAccount, bool submittedFlag)
        {
            var team = await _dbContext.Teams.SingleAsync();
            team.UseCustomAccountValidation = storedFlag;
            team.RechargeDebitNaturalAccount = storedAccount;
            await _dbContext.SaveChangesAsync();
            _dbContext.ChangeTracker.Clear();
            var controller = CreateController(isSystemUser: false);
            var model = CreateModel();
            model.UseCustomAccountValidation = submittedFlag;
            model.RechargeDebitNaturalAccount = "invalid-account";
            ValidateModel(controller, model);
            controller.ModelState.AddModelError(nameof(model.UseCustomAccountValidation), "Invalid boolean value.");

            var result = await controller.Edit(model);

            Assert.IsType<RedirectToActionResult>(result);
            var stored = await ReadTeam();
            Assert.Equal(model.Name, stored.Name);
            Assert.Equal(model.ContactEmail, stored.ContactEmail);
            Assert.Equal(storedFlag, stored.UseCustomAccountValidation);
            Assert.Equal(storedAccount, stored.RechargeDebitNaturalAccount);
            _aggieEnterpriseService.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task FailedAccountValidationDoesNotSaveAnyTeamChanges(bool serviceFails)
        {
            var controller = CreateController();
            var model = CreateModel();
            var validation = _aggieEnterpriseService.Setup(service => service.IsNaturalAccountValid("654321"));
            if (serviceFails)
            {
                validation.ThrowsAsync(new InvalidOperationException("Service unavailable"));
            }
            else
            {
                validation.ReturnsAsync(false);
            }

            var result = await controller.Edit(model);

            Assert.Same(model, Assert.IsType<ViewResult>(result).Model);
            Assert.NotEmpty(controller.ModelState[nameof(model.RechargeDebitNaturalAccount)].Errors);
            await AssertOriginalTeamIsUnchanged();
            _aggieEnterpriseService.Verify(service => service.IsNaturalAccountValid("654321"), Times.Once);
            _aggieEnterpriseService.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("12345")]
        [InlineData("1234567")]
        [InlineData("12-456")]
        [InlineData("１２３４５６")]
        public async Task MalformedAccountIsRejectedBeforeCallingAggieEnterprise(string account)
        {
            var controller = CreateController();
            var model = CreateModel();
            model.RechargeDebitNaturalAccount = account;
            ValidateModel(controller, model);

            var result = await controller.Edit(model);

            Assert.Same(model, Assert.IsType<ViewResult>(result).Model);
            Assert.NotEmpty(controller.ModelState[nameof(model.RechargeDebitNaturalAccount)].Errors);
            await AssertOriginalTeamIsUnchanged();
            _aggieEnterpriseService.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task IgnoringRestrictedAccountFieldsDoesNotIgnoreOtherValidationErrors(bool isSystemUser)
        {
            var controller = CreateController(isSystemUser);
            var model = CreateModel();
            model.UseCustomAccountValidation = false;
            model.RechargeDebitNaturalAccount = "invalid-account";
            ValidateModel(controller, model);
            controller.ModelState.AddModelError(nameof(model.Name), "Team name is required.");

            var result = await controller.Edit(model);

            Assert.IsType<ViewResult>(result);
            Assert.NotEmpty(controller.ModelState[nameof(model.Name)].Errors);
            await AssertOriginalTeamIsUnchanged();
            _aggieEnterpriseService.VerifyNoOtherCalls();
        }

        [Fact]
        public void NaturalAccountSearchRequiresTheSystemRoleAndGetMethod()
        {
            var method = typeof(SettingsController).GetMethod(nameof(SettingsController.SearchNaturalAccounts));

            Assert.NotNull(method.GetCustomAttribute<HttpGetAttribute>());
            var authorization = Assert.Single(method.GetCustomAttributes<AuthorizeAttribute>());
            Assert.Equal(ApplicationRoleCodes.Admin, authorization.Roles);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("12")]
        [InlineData("travel")]
        public async Task DisabledFeatureRejectsSearchWithoutCallingAggieEnterprise(string query)
        {
            var result = await CreateController(enableRechargeDebitNaturalAccount: false)
                .SearchNaturalAccounts(query);

            Assert.IsType<NotFoundResult>(result);
            _aggieEnterpriseService.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("12")]
        [InlineData(" 12 ")]
        public async Task SearchRejectsQueriesShorterThanThreeCharacters(string query)
        {
            var result = await CreateController().SearchNaturalAccounts(query);

            Assert.Equal(StatusCodes.Status400BadRequest, Assert.IsAssignableFrom<IStatusCodeActionResult>(result).StatusCode);
            _aggieEnterpriseService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task SearchRejectsQueriesLongerThanOneHundredCharacters()
        {
            var result = await CreateController().SearchNaturalAccounts(new string('a', 101));

            Assert.Equal(StatusCodes.Status400BadRequest, Assert.IsAssignableFrom<IStatusCodeActionResult>(result).StatusCode);
            _aggieEnterpriseService.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(3)]
        [InlineData(100)]
        public async Task SearchTrimsQueriesAndReturnsAccountCodesAndNames(int queryLength)
        {
            var query = new string('a', queryLength);
            _aggieEnterpriseService.Setup(service => service.SearchNaturalAccounts(query))
                .ReturnsAsync(new Dictionary<string, string> { ["654321"] = "Example natural account" });

            var result = await CreateController().SearchNaturalAccounts(" " + query + " ");

            var json = Assert.IsType<JsonResult>(result);
            var account = Assert.Single(JArray.FromObject(json.Value));
            Assert.Equal("654321", account.Value<string>("code"));
            Assert.Equal("Example natural account", account.Value<string>("name"));
            _aggieEnterpriseService.Verify(service => service.SearchNaturalAccounts(query), Times.Once);
            _aggieEnterpriseService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task SearchReturnsServiceUnavailableWhenAggieEnterpriseFails()
        {
            _aggieEnterpriseService.Setup(service => service.SearchNaturalAccounts("travel"))
                .ThrowsAsync(new InvalidOperationException("Service unavailable"));

            var result = await CreateController().SearchNaturalAccounts("travel");

            Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsAssignableFrom<IStatusCodeActionResult>(result).StatusCode);
            _aggieEnterpriseService.Verify(service => service.SearchNaturalAccounts("travel"), Times.Once);
            _aggieEnterpriseService.VerifyNoOtherCalls();
        }

        private SettingsController CreateController(
            bool isSystemUser = true, bool enableRechargeDebitNaturalAccount = true,
            ApplicationUserManager userManager = null)
        {
            var claims = isSystemUser
                ? new[] { new Claim(ClaimTypes.Role, ApplicationRoleCodes.Admin) }
                : Array.Empty<Claim>();
            var routeData = new RouteData();
            routeData.Values.Add("team", "alpha");
            var financeSettings = Options.Create(new FinanceSettings
            {
                EnableRechargeDebitNaturalAccount = enableRechargeDebitNaturalAccount,
            });
            return new SettingsController(_dbContext, null, null, null, userManager,
                _aggieEnterpriseService.Object, financeSettings)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")),
                    },
                    RouteData = routeData,
                },
            };
        }

        private static EditTeamViewModel CreateModel()
        {
            return new EditTeamViewModel
            {
                Name = "Updated Team",
                Slug = "alpha",
                ContactName = "Updated Contact",
                ContactEmail = "updated@example.com",
                ContactPhoneNumber = "530-555-0101",
                IsActive = true,
                UseCustomAccountValidation = true,
                RechargeDebitNaturalAccount = "654321",
            };
        }

        private static void ValidateModel(SettingsController controller, EditTeamViewModel model)
        {
            var errors = new List<ValidationResult>();
            Validator.TryValidateObject(model, new ValidationContext(model), errors, validateAllProperties: true);
            foreach (var error in errors)
            {
                foreach (var member in error.MemberNames)
                {
                    controller.ModelState.AddModelError(member, error.ErrorMessage);
                }
            }
        }

        private Task<Team> ReadTeam()
        {
            return _dbContext.Teams.AsNoTracking().SingleAsync();
        }

        private async Task AssertOriginalTeamIsUnchanged()
        {
            var stored = await ReadTeam();
            Assert.Equal("Alpha Team", stored.Name);
            Assert.Equal("Original Contact", stored.ContactName);
            Assert.Equal("original@example.com", stored.ContactEmail);
            Assert.True(stored.UseCustomAccountValidation);
            Assert.Equal("123456", stored.RechargeDebitNaturalAccount);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
            _connection.Dispose();
        }
    }
}
