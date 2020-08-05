using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
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
        public IActionResult Activity(string team) {
            // TODO: add in date range filters

            var invoices = _dbContext.Invoices
                .Where(i => i.Team.Slug == TeamSlug)
                .Where(i => i.CreatedAt >= DateTime.Now.AddMonths(-1))
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
        
        #endregion

        #region system reports
        /* system wide reports should use attribute routes */

        
        #endregion
    }
}
