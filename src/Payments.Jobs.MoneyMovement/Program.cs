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

namespace Payments.Jobs.MoneyMovement
{
    public class Program : JobBase
    {
        private static ILogger _log;

        public static void Main(string[] args)
        {
            // setup env
            Configure();

            // log run
            var jobRecord = new MoneyMovementJobRecord()
            {
                Id     = Guid.NewGuid().ToString(),
                Name   = MoneyMovementJob.JobName,
                RanOn  = DateTime.UtcNow,
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
            dbContext.MoneyMovementJobRecords.Add(jobRecord);
            dbContext.SaveChanges();

            try
            {
                // create job service
                var moneyMovementJob = provider.GetService<MoneyMovementJob>();

                // call method
                moneyMovementJob.FindBankReconcileTransactions(_log).GetAwaiter().GetResult();
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
            services.Configure<FinanceSettings>(Configuration.GetSection("Finance"));
            services.Configure<SlothSettings>(Configuration.GetSection("Sloth"));

            // db service
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
            );

            // required services
            services.AddTransient<ISlothService, SlothService>();

            services.AddSingleton<MoneyMovementJob>();

            return services.BuildServiceProvider();
        }
    }
}
