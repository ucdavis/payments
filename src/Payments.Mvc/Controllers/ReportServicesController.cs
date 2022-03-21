using System;
using System.Threading.Tasks;
using jsreport.AspNetCore;
using jsreport.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Mvc.Models.Roles;

namespace Payments.Mvc.Controllers
{
    public class ReportServicesController : ServicesController
    {
        private readonly ApplicationDbContext _dbContext;

        public ReportServicesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [MiddlewareFilter(typeof(JsReportPipeline))]
        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> TaxReport()
        {
            var invoices = await _dbContext.Invoices
                .Include(i => i.Account)
                .Include(i => i.Items)
                .Include(i => i.Team)
                .ToListAsync();

            HttpContext.JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .Configure(r => { });

            return View("Reports/_SalesLog", invoices);
        }
    }
}
