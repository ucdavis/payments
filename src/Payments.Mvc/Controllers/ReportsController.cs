using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Reports;
using Payments.Mvc.Models.ReportViewModels;

namespace Payments.Mvc.Controllers
{
    public class ReportsController : SuperController
    {
        private readonly ApplicationDbContext _dbContext;

        public ReportsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region team reports
        /* system wide reports should use attribute routes */

        [HttpGet]
        public IActionResult TeamTaxReport()
        {
            return View("TaxReport");
        }

        [HttpPost]
        public async Task<IActionResult> TeamTaxReport(TaxReportViewModel model)
        {
            // get base query for various timespan
            IQueryable<Invoice> query;
            if (string.Equals(model.Timespan, "month", StringComparison.OrdinalIgnoreCase))
            {
                query = new TaxReport(_dbContext)
                    .RunForMonth(model.StartDate);
            }
            else
            {
                query = new TaxReport(_dbContext)
                    .RunForFiscalYear(model.StartDate.Year);
            }

            // narrow to just this team
            query = query.Where(i => i.Team.Slug == TeamSlug);

            var invoices = await query
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            model.Invoices = invoices;

            return View("TaxReport", model);
        }
        #endregion

        #region system reports
        /* system wide reports should use attribute routes */

        [HttpGet("reports/tax-report")]
        public IActionResult TaxReportByMonth()
        {
            return View("TaxReport");
        }

        [HttpPost("reports/tax-report")]
        public async Task<IActionResult> TaxReportByMonth(TaxReportViewModel model)
        {
            // get base query for various timespan
            IQueryable<Invoice> query;
            if (string.Equals(model.Timespan, "month", StringComparison.OrdinalIgnoreCase))
            {
                query = new TaxReport(_dbContext)
                    .RunForMonth(model.StartDate);
            }
            else
            {
                query = new TaxReport(_dbContext)
                    .RunForFiscalYear(model.StartDate.Year);
            }

            var invoices = await query
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            model.Invoices = invoices;

            return View("TaxReport", model);
        }
        #endregion
    }
}
