using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Helpers;
using Payments.Mvc.Logging;
using Payments.Mvc.Models;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

namespace Payments.Mvc
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var isDevelopment = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "development", StringComparison.OrdinalIgnoreCase);
            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            //only add secrets in development
            if (isDevelopment)
            {
                builder.AddUserSecrets<Program>();
            }
            var configuration = builder.Build();

            LoggingConfiguration.Setup(configuration);

            try
            {
                Log.Information("Starting web host");
                var host = CreateHostBuilder(args).Build();

                Log.Information("Initializing DB");
                using (var scope = host.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                    var rolemanager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    var superuserSettings = scope.ServiceProvider.GetRequiredService<IOptions<SuperuserSettings>>();
                    var dbInitilizer = new DbInitializer(dbContext, userManager, rolemanager, superuserSettings);
#if DEBUG
                    var settings = scope.ServiceProvider.GetRequiredService<IOptions<Settings>>();
                    if (settings.Value.RebuildDb == "Yes")
                    {
                        Task.Run(() => dbInitilizer.Recreate()).Wait();
                    }
#endif
                    Task.Run(() => dbInitilizer.Initialize()).Wait();
                }

                host.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
