using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi.Models;
using Moq;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Mvc.Authorization;
using Payments.Mvc.Controllers;
using Payments.Mvc.Handlers;
using Payments.Mvc.Identity;
using Payments.Mvc.Models.Roles;
using Payments.Mvc.Models.TeamApiViewModels;
using Payments.Mvc.Swagger;
using Swashbuckle.AspNetCore.Swagger;
using Xunit;

namespace payments.Tests.ControllerTests
{
    [Trait("Category", "ControllerTests")]
    public class TeamsApiControllerTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ApplicationDbContext _dbContext;

        public TeamsApiControllerTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;
            _dbContext = new ApplicationDbContext(options);
            _dbContext.Database.EnsureCreated();
            _dbContext.Teams.AddRange(
                new Team { Id = 1, Name = "Alpha Team", Slug = "alpha", ApiKey = "alpha-test-key" },
                new Team { Id = 2, Name = "Beta Team", Slug = "beta", ApiKey = "beta-test-key" },
                new Team { Id = 3, Name = "Inactive Team", Slug = "inactive", ApiKey = "inactive-test-key", IsActive = false });
            _dbContext.SaveChanges();
            _dbContext.ChangeTracker.Clear();
        }

        [Theory]
        [InlineData("alpha-test-key", "Alpha Team", "alpha")]
        [InlineData("beta-test-key", "Beta Team", "beta")]
        public async Task GetReturnsTheTeamAssociatedWithTheApiKey(string apiKey, string name, string slug)
        {
            var nextCalled = false;
            var httpContext = await AuthenticateApiKey(apiKey, _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });
            Assert.True(nextCalled);
            Assert.True(await IsAuthorized(httpContext.User));
            var controller = CreateController(httpContext);

            var result = await controller.Get();

            var team = Assert.IsType<TeamResult>(result.Value);
            Assert.Equal(name, team.Name);
            Assert.Equal(slug, team.Slug);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("invalid-test-key", false)]
        [InlineData("inactive-test-key", false)]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData(" ", true)]
        [InlineData("invalid-test-key", true)]
        [InlineData("inactive-test-key", true)]
        public async Task TeamEndpointRejectsUnusableKeysWithoutLoginFallback(string apiKey, bool signedIn)
        {
            var nextCalled = false;
            var context = CreateHttpContext(typeof(TeamsApiController));
            if (signedIn)
            {
                context.User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "cookie-user") }, "Cookies"));
            }
            var httpContext = await AuthenticateApiKey(apiKey, _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            }, context);

            Assert.Equal(StatusCodes.Status401Unauthorized, httpContext.Response.StatusCode);
            Assert.Equal(0L, httpContext.Response.ContentLength);
            Assert.False(httpContext.Response.Headers.ContainsKey("Location"));
            Assert.False(nextCalled);
            Assert.False(await IsAuthorized(httpContext.User));
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("invalid-test-key", false)]
        [InlineData("inactive-test-key", false)]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData(" ", true)]
        [InlineData("invalid-test-key", true)]
        [InlineData("inactive-test-key", true)]
        public async Task OtherEndpointsKeepLoginFallback(string apiKey, bool hasControllerEndpoint)
        {
            var nextCalled = false;
            var context = CreateHttpContext(hasControllerEndpoint ? typeof(InvoicesApiController) : null);

            var httpContext = await AuthenticateApiKey(apiKey, nextContext =>
            {
                nextCalled = true;
                nextContext.Response.Redirect("/Account/Login");
                return Task.CompletedTask;
            }, context);

            Assert.True(nextCalled);
            Assert.Equal(StatusCodes.Status302Found, httpContext.Response.StatusCode);
            Assert.Equal("/Account/Login", httpContext.Response.Headers["Location"].ToString());
            Assert.False(await IsAuthorized(httpContext.User));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task BlankStoredKeyCannotAuthorizeTheTeamEndpoint(string apiKey)
        {
            _dbContext.Teams.Single(team => team.Id == 1).ApiKey = apiKey;
            await _dbContext.SaveChangesAsync();

            var httpContext = await AuthenticateApiKey(apiKey);

            Assert.Equal(StatusCodes.Status401Unauthorized, httpContext.Response.StatusCode);
            Assert.False(await IsAuthorized(httpContext.User));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("invalid")]
        [InlineData("2147483648")]
        [InlineData("999")]
        public async Task GetReturnsNotFoundWhenTheTeamClaimCannotResolveATeam(string teamId)
        {
            var claims = teamId == null
                ? Array.Empty<Claim>()
                : new[] { new Claim(ClaimTypes.Sid, teamId) };
            var controller = CreateController(new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims))
            });

            var result = await controller.Get();

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void SwaggerDocumentsTheTeamEndpointResponseAndApiKeySecurity()
        {
            var securityScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" },
                Type = SecuritySchemeType.ApiKey,
                Name = ApiKeyMiddleware.HeaderKey,
                In = ParameterLocation.Header,
                Scheme = "ApiKey"
            };
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(Mock.Of<IWebHostEnvironment>());
            services.AddControllers().AddApplicationPart(typeof(TeamsApiController).Assembly);
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Payments API", Version = "v1" });
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Payments.Mvc.xml"));
                options.AddSecurityDefinition("ApiKey", securityScheme);
                options.OperationFilter<SecurityRequirementsOperationFilter>(securityScheme);
            });
            using var provider = services.BuildServiceProvider();

            var swagger = provider.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");

            var operation = swagger.Paths["/api/team"].Operations[OperationType.Get];
            Assert.Contains("team name and slug", operation.Summary);
            Assert.Contains("Authorization header", operation.Description);
            Assert.Contains("401 Unauthorized without redirecting to login", operation.Description);
            Assert.Empty(operation.Parameters);
            Assert.Contains("200", operation.Responses.Keys);
            Assert.Contains("404", operation.Responses.Keys);
            Assert.Contains("401", operation.Responses.Keys);
            Assert.Contains("403", operation.Responses.Keys);
            var security = Assert.Single(Assert.Single(operation.Security));
            Assert.Equal("ApiKey", security.Key.Reference.Id);
            Assert.Contains(PolicyCodes.ApiKey, security.Value);
            Assert.Equal(ApiKeyMiddleware.HeaderKey, swagger.Components.SecuritySchemes["ApiKey"].Name);
            Assert.Equal(ParameterLocation.Header, swagger.Components.SecuritySchemes["ApiKey"].In);
            var response = operation.Responses["200"].Content["application/json"].Schema;
            var schema = swagger.Components.Schemas[response.Reference.Id];
            Assert.Equal(new[] { "name", "slug" }, schema.Properties.Keys.OrderBy(key => key, StringComparer.Ordinal));
            Assert.Equal("string", schema.Properties["name"].Type);
            Assert.Equal("string", schema.Properties["slug"].Type);
            Assert.Contains("display name", schema.Properties["name"].Description);
            Assert.Contains("URL slug", schema.Properties["slug"].Description);
        }

        private async Task<DefaultHttpContext> AuthenticateApiKey(
            string apiKey, RequestDelegate next = null, DefaultHttpContext context = null)
        {
            context ??= CreateHttpContext(typeof(TeamsApiController));
            if (apiKey != null)
            {
                context.Request.Headers[ApiKeyMiddleware.HeaderKey] = apiKey;
            }

            using var provider = new ServiceCollection().BuildServiceProvider();
            var app = new ApplicationBuilder(provider);
            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            app.Use(nextMiddleware => httpContext =>
                new ApiKeyMiddleware(nextMiddleware, NullLoggerFactory.Instance).Invoke(httpContext, _dbContext));
            app.Run(next ?? (_ => Task.CompletedTask));
            await app.Build()(context);
            return context;
        }

        private static DefaultHttpContext CreateHttpContext(Type controllerType)
        {
            var context = new DefaultHttpContext();
            if (controllerType != null)
            {
                context.SetEndpoint(new Endpoint(_ => Task.CompletedTask,
                    new EndpointMetadataCollection(new ControllerActionDescriptor
                    {
                        ControllerTypeInfo = controllerType.GetTypeInfo()
                    }), controllerType.Name));
            }
            return context;
        }

        private static async Task<bool> IsAuthorized(ClaimsPrincipal user)
        {
            var authorization = new AuthorizationHandlerContext(
                new[] { new VerifyApiKeyRequirement() }, user, null);
            await new VerifyApiKeyRequirementHandler().HandleAsync(authorization);
            return authorization.HasSucceeded;
        }

        private TeamsApiController CreateController(HttpContext httpContext)
        {
            return new TeamsApiController(_dbContext)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };
        }

        public void Dispose()
        {
            _dbContext.Dispose();
            _connection.Dispose();
        }
    }
}
