using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Mvc.Models.InvoiceViewModels;
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
        public IActionResult Activity(string team)
        {
            // TODO: add in date range filters

            var invoices = _dbContext.Invoices
                .Include(i => i.Account)
                .Where(i => i.Team.Slug == TeamSlug)
                .Where(i => i.CreatedAt >= DateTime.Now.AddMonths(-12))
                .OrderByDescending(i => i.Id)
                .AsNoTracking()
                .ToList();

            var model = new InvoiceListViewModel()
            {
                Invoices = invoices,
                Filter = null
            };

            return View(model);
        }

        /// Shows all unpaid invoices, grouping by customer to show how much is unpaid based on sent date brackets
        public IActionResult Aging(string team)
        {
            // TODO: add in date range filters
            var invoices = _dbContext.Invoices
                .Where(i => i.Team.Slug == TeamSlug)
                .Where(i => !i.Paid && i.Sent) // just show invoices that have been sent but not paid
                .AsNoTracking()
                .ToList();

            var byCustomer = invoices.GroupBy(i => i.CustomerEmail);

            var agingTotals = byCustomer.Select(c => new CustomerAgingTotals
            {
                CustomerEmail = c.Key,
                OneMonth = c.Where(i => i.SentAt.Value >= DateTime.Now.AddMonths(-1)).Sum(i => i.CalculatedTotal),
                TwoMonths = c.Where(i => i.SentAt.Value >= DateTime.Now.AddMonths(-2) && i.SentAt.Value < DateTime.Now.AddMonths(-1)).Sum(i => i.CalculatedTotal),
                ThreeMonths = c.Where(i => i.SentAt.Value >= DateTime.Now.AddMonths(-3) && i.SentAt.Value < DateTime.Now.AddMonths(-2)).Sum(i => i.CalculatedTotal),
                FourMonths = c.Where(i => i.SentAt.Value >= DateTime.Now.AddMonths(-4) && i.SentAt.Value < DateTime.Now.AddMonths(-3)).Sum(i => i.CalculatedTotal),
                FourToSixMonths = c.Where(i => i.SentAt.Value >= DateTime.Now.AddMonths(-6) && i.SentAt.Value < DateTime.Now.AddMonths(-4)).Sum(i => i.CalculatedTotal),
                SixToTwelveMonths = c.Where(i => i.SentAt.Value >= DateTime.Now.AddMonths(-12) && i.SentAt.Value < DateTime.Now.AddMonths(-6)).Sum(i => i.CalculatedTotal),
                OneToTwoYears = c.Where(i => i.SentAt.Value >= DateTime.Now.AddYears(-2) && i.SentAt.Value < DateTime.Now.AddYears(-1)).Sum(i => i.CalculatedTotal),
                OverTwoYears = c.Where(i => i.SentAt.Value < DateTime.Now.AddYears(-2)).Sum(i => i.CalculatedTotal),
                Total = c.Sum(i => i.CalculatedTotal)
            }).ToArray();

            return View(agingTotals);
        }

        #endregion

        #region system reports
        /* system wide reports should use attribute routes */


        #endregion
    }
}
