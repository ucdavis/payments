using System;
using System.IO;
using System.Threading.Tasks;
using jsreport.AspNetCore;
using jsreport.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Models.Storage;
using Payments.Core.Services;

namespace Payments.Mvc.Controllers
{
    public class PdfController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IStorageService _storageService;
        private readonly IJsReportMVCService _jsReportMvcService;

        public PdfController(ApplicationDbContext dbContext, IStorageService storageService, IJsReportMVCService jsReportMvcService)
        {
            _dbContext = dbContext;
            _storageService = storageService;
            _jsReportMvcService = jsReportMvcService;
        }

        [HttpGet("/pdf/{id}")]
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

            // look for the file on storage server first
            var identifier = $"invoice-{invoice.LinkId}-{invoice.GetFormattedId()}";

            var file = await _storageService.DownloadFile(identifier);
            if (await file.ExistsAsync())
            {
                var stream = await file.OpenReadAsync();
                return new FileStreamResult(stream, file.Properties.ContentType);
            }

            var footer = await _jsReportMvcService.RenderViewToStringAsync(HttpContext, RouteData, "Footer", invoice);

            var request = new RenderRequest()
            {
                Template = new Template()
                {
                    Recipe = Recipe.ChromePdf,
                    Engine = Engine.None,
                    Chrome = new Chrome()
                    {
                        DisplayHeaderFooter = true,
                        HeaderTemplate = "<div></div>", // use empty header
                        FooterTemplate = footer,
                        MarginTop = "1in",
                        MarginBottom = "2.5in",
                        MarginLeft = "25px",
                        MarginRight = "25px",
                    },
                },
                Options = new RenderOptions()
                {
                    Debug = new DebugOptions()
                    {
                        LogsToResponseHeader = true,
                    },
                },
            };

            // generate pdf
            var report = await _jsReportMvcService.RenderViewAsync(HttpContext, request, RouteData, "Invoice", invoice);
            var logs = report.Meta.Logs;

            // save result to storage server for retrieval
            var uploadStream = new MemoryStream();
            await report.Content.CopyToAsync(uploadStream);
            var upload = new UploadRequest()
            {
                Identifier = identifier,
                ContentType = report.Meta.ContentType,
                Data = uploadStream,
            };
            await _storageService.UploadFiles(upload);

            // reset stream and return
            report.Content.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(report.Content, report.Meta.ContentType);
        }

        [HttpGet("/receipt/{id}")]
        public async Task<ActionResult> Receipt(string id)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Team)
                .Include(i => i.PaymentEvents)
                .FirstOrDefaultAsync(i => i.LinkId == id);

            if (invoice == null || string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            if (!invoice.Paid)
            {
                return NotFound();
            }

            // look for the file on storage server first
            var identifier = $"receipt-{invoice.LinkId}-{invoice.GetFormattedId()}";

            var file = await _storageService.DownloadFile(identifier);
            if (await file.ExistsAsync())
            {
                var stream = await file.OpenReadAsync();
                return new FileStreamResult(stream, file.Properties.ContentType);
            }

            var footer = await _jsReportMvcService.RenderViewToStringAsync(HttpContext, RouteData, "Footer", invoice);

            var request = new RenderRequest()
            {
                Template = new Template()
                {
                    Recipe = Recipe.ChromePdf,
                    Engine = Engine.None,
                    Chrome = new Chrome()
                    {
                        DisplayHeaderFooter = true,
                        HeaderTemplate = "<div></div>", // use empty header
                        FooterTemplate = footer,
                        MarginTop = "1in",
                        MarginBottom = "2.5in",
                        MarginLeft = "25px",
                        MarginRight = "25px",
                    },
                },
                Options = new RenderOptions()
                {
                    Debug = new DebugOptions()
                    {
                        LogsToResponseHeader = true,
                    },
                },
            };

            // generate pdf
            var report = await _jsReportMvcService.RenderViewAsync(HttpContext, request, RouteData, "Receipt", invoice);
            var logs = report.Meta.Logs;

            // save result to storage server for retrieval
            var uploadStream = new MemoryStream();
            await report.Content.CopyToAsync(uploadStream);
            var upload = new UploadRequest()
            {
                Identifier = identifier,
                ContentType = report.Meta.ContentType,
                Data = uploadStream,
            };
            await _storageService.UploadFiles(upload);

            // reset stream and return
            report.Content.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(report.Content, report.Meta.ContentType);
        }
    }
}
