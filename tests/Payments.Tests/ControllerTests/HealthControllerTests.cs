using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Payments.Core.Data;
using Payments.Mvc.Controllers;
using Xunit;

namespace payments.Tests.ControllerTests
{
    public class HealthControllerTests
    {
        [Theory]
        [InlineData("Data Source=:memory:", HttpStatusCode.OK, "Healthy")]
        [InlineData("Data Source=/missing-payments-health-test/database.db;Mode=ReadOnly", HttpStatusCode.ServiceUnavailable, "Unhealthy")]
        public async Task AnonymousRequestReportsDatabaseAvailabilityWithoutDetails(
            string connectionString, HttpStatusCode expectedStatus, string expectedBody)
        {
            using var server = new TestServer(new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
                    services.AddControllers().AddApplicationPart(typeof(HealthController).Assembly);
                    services.AddAuthentication("test").AddCookie("test");
                    services.AddAuthorization(options => options.FallbackPolicy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser().Build());
                })
                .Configure(app =>
                {
                    app.UseStatusCodePagesWithReExecute("/Error/{0}");
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints => endpoints.MapControllers());
                }));
            using var client = server.CreateClient();
            using var response = await client.GetAsync("/health");
            Assert.Equal(expectedStatus, response.StatusCode);
            Assert.Equal(expectedBody, await response.Content.ReadAsStringAsync());
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.True(response.Headers.CacheControl.NoStore);
        }
    }
}
