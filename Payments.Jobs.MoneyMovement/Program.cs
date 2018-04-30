using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Payments.Core.Data;
using Payments.Core.Models.Configuration;
using Payments.Core.Services;
using Payments.Jobs.Core;
using Serilog;

namespace Payments.Jobs.MoneyMovement
{
    public class Program : JobBase
    {
        private static ILogger _log;

        public static void Main(string[] args)
        {
            _log = Log.ForContext("jobid", Guid.NewGuid());

            var assembyName = typeof(Program).Assembly.GetName();
            _log.Information("Running {job} build {build}", assembyName.Name, assembyName.Version);

            // setup di
            var provider = ConfigureServices();

            // create services
            SlothService = provider.GetService<ISlothService>();
            DbContext = provider.GetService<ApplicationDbContext>();

            // call methods
            FindBankReconcileTransactions();
        }

        private static ServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
            );
            services.Configure<SlothSettings>(Configuration.GetSection("Sloth"));

            services.AddTransient<ISlothService, SlothService>();
            return services.BuildServiceProvider();
        }

        public static ISlothService SlothService { get; private set; }
        public static ApplicationDbContext DbContext { get; private set; }

        public static void FindBankReconcileTransactions()
        {
        }
    }
}
