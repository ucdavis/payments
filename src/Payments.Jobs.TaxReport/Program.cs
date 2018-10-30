using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Jobs;
using Payments.Core.Models.Configuration;
using Payments.Core.Services;
using Payments.Jobs.Core;
using Serilog;

namespace Payments.Jobs.TaxReport
{
    public class Program : JobBase
    {
        private static ILogger _log;

        public static void Main(string[] args)
        {
            // setup env
            Configure();

            // log run
            var jobRecord = new TaxReportJobRecord()
            {
                Id = Guid.NewGuid().ToString(),
                Name = TaxReportJob.JobName,
                RanOn = DateTime.UtcNow,
                Status = "Running",
            };

            _log = Log.Logger
                .ForContext("jobname", jobRecord.Name)
                .ForContext("jobid", jobRecord.Id);

            var assembyName = typeof(Program).Assembly.GetName();
            _log.Information("Running {job} build {build}", assembyName.Name, assembyName.Version);

            // setup di
            var provider = ConfigureServices();
            var dbContext = provider.GetService<ApplicationDbContext>();

            // save log to db
            dbContext.TaxReportJobRecords.Add(jobRecord);
            dbContext.SaveChanges();

            try
            {
                // create job service
                var taxReportJob = provider.GetService<TaxReportJob>();

                // call method
                taxReportJob.EmailMonthlyTaxReport(_log).GetAwaiter().GetResult();
            }
            finally
            {
                // record status
                jobRecord.Status = "Finished";
                dbContext.SaveChanges();
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            // options files
            services.Configure<SparkpostSettings>(Configuration.GetSection("Sparkpost"));

            // db service
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
            );

            // required services
            services.AddTransient<IEmailService, SparkpostEmailService>();

            services.AddSingleton<TaxReportJob>();

            return services.BuildServiceProvider();
        }
    }
}
