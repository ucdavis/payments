using Destructurama;
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
        public static void ConfigureLogging()
        {
            if (_loggingSetup) return; //only setup logging once

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