using Destructurama;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Exceptions;

namespace Payments.Web.Infrastructure
{
    /// <summary>
    /// Configure Application Logging
    /// </summary>
    public static class Logging
    {
        private static bool _loggingSetup;

        /// <summary>
        /// Configure Application Logging
        /// </summary>
        public static void ConfigureLogging(IConfigurationRoot configuration)
        {
            if (_loggingSetup) return; //only setup logging once

            // setup global keys
            StackifyLib.Logger.GlobalApiKey = configuration.GetValue<string>("Stackify.ApiKey");
            StackifyLib.Logger.GlobalAppName = configuration.GetValue<string>("Stackify.AppName");
            StackifyLib.Logger.GlobalEnvironment = configuration.GetValue<string>("Stackify.Environment");

            Log.Logger = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .Enrich.WithExceptionDetails()
                .Enrich.FromLogContext()
                .WriteTo.Stackify()
                .CreateLogger();

            _loggingSetup = true;
        }
    }
}