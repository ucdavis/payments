using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Extensions;
using Payments.Core.Jobs;
using Payments.Mvc.Logging;
using Payments.Mvc.Models.JobViewModels;
using Payments.Mvc.Models.Roles;
using Payments.Mvc.Services;

namespace Payments.Mvc.Controllers
{
    [Authorize(Roles = ApplicationRoleCodes.Admin)]
    public class JobsController : SuperController
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IBackgroundTaskQueue _queue;

        public JobsController(ApplicationDbContext dbContext, IBackgroundTaskQueue queue)
        {
            _dbContext = dbContext;
            _queue = queue;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Money Movement
        public async Task<IActionResult> MoneyMovement()
        {
            return View();
        }

        public async Task<IActionResult> MoneyMovementDetails(string id)
        {
            var record = await _dbContext.MoneyMovementJobRecords
                .Include(r => r.Logs)
                .FirstOrDefaultAsync(r => r.Id == id);

            return View(record);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public async Task<IActionResult> MoneyMovementRecords(DateTime start, DateTime end)
        {
            // fetch records
            var records = await _dbContext.MoneyMovementJobRecords
                .Where(r => r.RanOn >= start && r.RanOn <= end)
                .OrderBy(r => r.RanOn)
                .ToListAsync();

            // format for js, add offset to local, reset to js epoch
            var events = records.Select(r => new
            {
                id     = r.Id,
                title  = $"{r.Name} - {r.RanOn.ToPacificTime():MMM dd, h:mm tt}",
                @class = "event-success",
                url    = Url.Action(nameof(MoneyMovementDetails), new { id = r.Id }),
                start  = r.RanOn.ToPacificTime(),
            }).ToList();

            return new JsonResult(events);
        }

        [HttpPost]
        public async Task<IActionResult> MoneyMovementRun()
        {
            // log run
            var jobRecord = new MoneyMovementJobRecord()
            {
                Id     = Guid.NewGuid().ToString(),
                Name   = MoneyMovementJob.JobName,
                RanOn  = DateTime.UtcNow,
                Status = "Running",
            };
            _dbContext.MoneyMovementJobRecords.Add(jobRecord);
            await _dbContext.SaveChangesAsync();

            // build task and add to queue
            _queue.QueueBackgroundWorkItem(async (token, serviceProvider) =>
            {
                var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
                var moneyMovementJob = serviceProvider.GetRequiredService<MoneyMovementJob>();

                // find job record
                var scopedRecord = await dbContext.MoneyMovementJobRecords.FindAsync(jobRecord.Id);

                // build custom logger
                var log = LoggingConfiguration.GetJobConfiguration()
                    .CreateLogger()
                    .ForContext("jobname", scopedRecord.Name)
                    .ForContext("jobid", scopedRecord.Id);

                try
                {
                    // call methods
                    log.Information("Starting Job");
                    await moneyMovementJob.FindBankReconcileTransactions(log);
                    await moneyMovementJob.FindIncomeTransactions(log);
                }
                finally
                {
                    // record status
                    scopedRecord.Status = "Finished";
                    await dbContext.SaveChangesAsync(token);
                }
            });

            return RedirectToAction(nameof(MoneyMovementDetails), new { id = jobRecord.Id });
        }
        #endregion
    }
}
