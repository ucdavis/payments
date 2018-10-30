using System;
using System.Linq;
using System.Threading.Tasks;
using jsreport.AspNetCore;
using jsreport.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Extensions;
using Payments.Mvc.Models.ReportViewModels;
using Payments.Mvc.Models.Roles;

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

        [HttpGet]
        [Authorize(Policy = PolicyCodes.TeamEditor)]
        public IActionResult TaxReport()
        {
            var lastFiscalYear = DateTime.UtcNow.FiscalYear() - 1;

            var model = new TaxReportViewModel()
            {
                FiscalYear = lastFiscalYear,
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = PolicyCodes.TeamEditor)]
        public IActionResult TaxReport(TaxReportViewModel model)
        {
            // get all invoices for team
            var query = _dbContext.Invoices
                .AsQueryable()
                .Include(i => i.Items)
                .Where(i => i.Team.Slug == TeamSlug);

            // for this fiscal year
            var fiscalStart = new DateTime(model.FiscalYear - 1, 7, 1);
            var fiscalEnd = new DateTime(model.FiscalYear, 7, 1);

            query = query.Where(i =>
                   i.PaidAt >= fiscalStart
                && i.PaidAt <= fiscalEnd);

            var invoices = query
                .OrderByDescending(i => i.Id)
                .ToList();

            model.Invoices = invoices;

            return View(model);
        }
    }
}
