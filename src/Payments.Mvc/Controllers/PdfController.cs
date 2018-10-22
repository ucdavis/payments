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
        public async Task<ActionResult> Invoice(string id, bool debug = false, bool store = false)
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
                        FooterTemplate = footer,
                        MarginTop = "1in",
                        MarginBottom = "4in",
                        MarginLeft = "25px",
                        MarginRight = "25px"
                    };
                });

            if (store)
            {
                HttpContext.JsReportFeature()
                    .OnAfterRender((r) => {
                        using (var file = System.IO.File.Open("report.pdf", FileMode.Create))
                        {
                            r.Content.CopyTo(file);
                        }
                        r.Content.Seek(0, SeekOrigin.Begin);
                    });
            }

            if (debug)
            {
                HttpContext.JsReportFeature()
                    .DebugLogsToResponse();
            }

            return View(invoice);
        }
    }
}
