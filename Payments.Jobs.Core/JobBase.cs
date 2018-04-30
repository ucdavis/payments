using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Jobs.Core.Logging;

namespace Payments.Jobs.Core
{
    public abstract class JobBase
    {
        public static IConfigurationRoot Configuration { get; set; }

        protected JobBase()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.Equals(environmentName, "development", StringComparison.OrdinalIgnoreCase))
            {
                builder.AddUserSecrets<JobBase>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            LogHelper.ConfigureLogging(Configuration);
        }
    }
}
