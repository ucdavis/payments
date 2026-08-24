using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Mvc.Controllers;
using Payments.Mvc.Models.Roles;
using Payments.Mvc.Models.SearchViewModels;
using Shouldly;
using Xunit;

namespace payments.Tests.ControllerTests
{
    [Trait("Category", "ControllerTests")]
    public class SearchControllerTests : IDisposable
    {
        private const string SearchEmail = "customer@example.com";
        private readonly SqliteConnection _connection;
        private readonly ApplicationDbContext _dbContext;

        public SearchControllerTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;
            _dbContext = new ApplicationDbContext(options);
            _dbContext.Database.EnsureCreated();
            _dbContext.Invoices.AddRange(
                CreateInvoice(1, "alpha"),
                CreateInvoice(2, "beta"));
            _dbContext.SaveChanges();
            _dbContext.ChangeTracker.Clear();
        }

        [Fact]
        public void QueryWithoutTeamReturnsInvoicesFromAllTeamsForSystemUser()
        {
            var controller = CreateController(isSystemUser: true);

            var result = controller.Query(SearchEmail);

            var model = GetModel(result);
            model.Invoices.Select(i => i.Id).ShouldBe(new[] { 2, 1 });
            model.Invoices.Select(i => i.Team.Slug).ShouldBe(new[] { "beta", "alpha" });
        }

        [Fact]
        public void QueryWithoutTeamRetainsTeamScopedBehaviorForNonSystemUser()
        {
            var controller = CreateController(isSystemUser: false);

            var result = controller.Query(SearchEmail);

            GetModel(result).Invoices.ShouldBeEmpty();
        }

        [Fact]
        public void QueryWithTeamRemainsTeamScopedForSystemUser()
        {
            var controller = CreateController(isSystemUser: true, teamSlug: "alpha");

            var result = controller.Query(SearchEmail);

            var invoice = GetModel(result).Invoices.ShouldHaveSingleItem();
            invoice.Id.ShouldBe(1);
        }

        private SearchController CreateController(bool isSystemUser, string teamSlug = null)
        {
            var claims = isSystemUser
                ? new[] { new Claim(ClaimTypes.Role, ApplicationRoleCodes.Admin) }
                : new Claim[0];
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")),
            };
            var routeData = new RouteData();
            if (teamSlug != null)
            {
                routeData.Values.Add("team", teamSlug);
            }

            return new SearchController(_dbContext)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext,
                    RouteData = routeData,
                },
            };
        }

        private static Invoice CreateInvoice(int id, string teamSlug)
        {
            return new Invoice
            {
                Id = id,
                CustomerEmail = SearchEmail,
                Team = new Team { Name = teamSlug, Slug = teamSlug },
            };
        }

        private static SearchResultsViewModel GetModel(IActionResult result)
        {
            return result.ShouldBeOfType<ViewResult>().Model.ShouldBeOfType<SearchResultsViewModel>();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
            _connection.Dispose();
        }
    }
}
