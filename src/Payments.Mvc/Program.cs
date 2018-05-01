using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Helpers;

namespace Payments.Mvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
#if DEBUG            
            using (var scope = host.Services.CreateScope())
            {
                var settings = scope.ServiceProvider.GetRequiredService<IOptions<Settings>>();
                if (settings.Value.RebuildDb == "Yes")
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                    var rolemanager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    var dbInitilizer = new DbInitializer(context, userManager, rolemanager);
                    Task.Run(() => dbInitilizer.RecreateAndInitialize()).Wait();
                }
            }
#endif

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
