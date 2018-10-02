using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Extensions;
using Payments.Mvc.Models.ReportViewModels;
using Payments.Mvc.Models.Roles;

namespace Payments.Mvc.Controllers
{
    [Authorize(Policy = PolicyCodes.TeamEditor)]
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
        public IActionResult TaxReport(TaxReportViewModel model)
        {
            // get all invoices for team
            var query = _dbContext.Invoices
                .AsQueryable()
                .Include(i => i.Items)
                .Include(i => i.Payment)
                .Where(i => i.Team.Slug == TeamSlug);

            // for this fiscal year
            var fiscalStart = new DateTime(model.FiscalYear - 1, 7, 1);
            var fiscalEnd = new DateTime(model.FiscalYear, 7, 1);

            query = query.Where(i =>
                   i.Payment.OccuredAt >= fiscalStart
                && i.Payment.OccuredAt <= fiscalEnd);

            var invoices = query
                .OrderByDescending(i => i.Id)
                .ToList();

            model.Invoices = invoices;

            return View(model);
        }
    }
}
