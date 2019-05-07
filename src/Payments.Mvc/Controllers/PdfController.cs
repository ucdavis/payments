using System;
using System.IO;
using System.Threading.Tasks;
using jsreport.AspNetCore;
using jsreport.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;

namespace Payments.Mvc.Controllers
{
    public class PdfController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IJsReportMVCService _jsReportMvcService;

        public PdfController(ApplicationDbContext dbContext, IJsReportMVCService jsReportMvcService)
        {
            _dbContext = dbContext;
            _jsReportMvcService = jsReportMvcService;
        }

        [HttpGet("/pdf/{id}")]
        [MiddlewareFilter(typeof(JsReportPipeline))]
        public async Task<ActionResult> Invoice(string id)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Team)
                .FirstOrDefaultAsync(i => i.LinkId == id);

            if (invoice == null || string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var footer = await _jsReportMvcService.RenderViewToStringAsync(HttpContext, RouteData, "Footer", invoice);

            HttpContext.JsReportFeature()
                .Recipe(Recipe.ChromePdf)
                .Configure(r =>
                {
                    r.Options.Debug.LogsToResponseHeader = true;

                    r.Template.Chrome = new Chrome()
                    {
                        DisplayHeaderFooter = true,
                        HeaderTemplate = "<div></div>", // use empty header
                        FooterTemplate = footer,
                        MarginTop = "1in",
                        MarginBottom = "2.5in",
                        MarginLeft = "25px",
                        MarginRight = "25px"
                    };
                });

            return View(invoice);
        }
    }
}
