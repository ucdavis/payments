using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Helpers;
using Payments.Mvc.Models;

namespace Payments.Mvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
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
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
