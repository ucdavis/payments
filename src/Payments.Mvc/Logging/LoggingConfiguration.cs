using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.MSSqlServer;
using StackifyLib;

namespace Payments.Mvc.Logging
{
    public static class LoggingConfiguration
    {
        private static bool _loggingSetup;

        private static IConfigurationRoot _configuration;

        /// <summary>
        /// Configure Application Logging
        /// </summary>
        public static void Setup(IConfigurationRoot configuration)
        {
            if (_loggingSetup) return; //only setup logging once

            // save configuration for later calls
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // create global logger with standard configuration
            Log.Logger = GetConfiguration().CreateLogger();

#if DEBUG
            Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
#endif

            _loggingSetup = true;
        }

        /// <summary>
        /// Get standard logger configuration
        /// </summary>
        /// <returns></returns>
        public static LoggerConfiguration GetConfiguration()
        {
            if (_configuration == null) throw new InvalidOperationException("Call Setup() before requesting a Logger Configuration"); ;

            var loggingSection = _configuration.GetSection("Stackify");

            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                // .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning) // uncomment this to hide EF core general info logs
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithClientIp()
                .Enrich.WithClientAgent()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("Application", loggingSection.GetValue<string>("AppName"))
                .Enrich.WithProperty("AppEnvironment", loggingSection.GetValue<string>("Environment"))
                .WriteTo.Console()
                .WriteToStackifyCustom();


            // add in elastic search sink if the uri is valid
            if (Uri.TryCreate(loggingSection.GetValue<string>("ElasticUrl"), UriKind.Absolute, out var elasticUri))
            {
                logConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(elasticUri)
                {
                    IndexFormat = "aspnet-payments-{0:yyyy.MM}"
                });
            }

            return logConfig;
        }

        /// <summary>
        /// Get standard-plus-sql logger configuration
        /// </summary>
        /// <returns></returns>
        public static LoggerConfiguration GetJobConfiguration()
        {
            return GetConfiguration()
                .WriteToSqlCustom();
        }

        private static LoggerConfiguration WriteToStackifyCustom(this LoggerConfiguration logConfig)
        {
            if (!_loggingSetup)
            {
                _configuration.ConfigureStackifyLogging();
            }

            return logConfig.WriteTo.Stackify();
        }

        private static LoggerConfiguration WriteToSqlCustom(this LoggerConfiguration logConfig)
        {
            var columnOptions = new ColumnOptions();

            // xml column
            columnOptions.Store.Remove(StandardColumn.Properties);

            // json column
            columnOptions.Store.Add(StandardColumn.LogEvent);
            columnOptions.LogEvent.ExcludeAdditionalProperties = true;

            // special columns for indexing
            columnOptions.AdditionalColumns = new List<SqlColumn>()
            {
                new SqlColumn {ColumnName = "Source", AllowNull = true, DataType = SqlDbType.NVarChar, DataLength = 128},
                new SqlColumn {ColumnName = "CorrelationId", AllowNull = true, DataType = SqlDbType.NVarChar, DataLength = 50},
                new SqlColumn {ColumnName = "JobName", AllowNull = true, DataType = SqlDbType.NVarChar, DataLength = 50},
                new SqlColumn {ColumnName = "JobId", AllowNull = true, DataType = SqlDbType.NVarChar, DataLength = 50},
            };

            return logConfig
                .WriteTo.MSSqlServer(
                    connectionString: _configuration.GetConnectionString("DefaultConnection"),
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    columnOptions: columnOptions,
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "Logs",
                    }
                );
        }
    }
}
