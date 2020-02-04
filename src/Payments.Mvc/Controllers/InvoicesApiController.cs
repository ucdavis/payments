using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Models.History;
using Payments.Core.Models.Invoice;
using Payments.Core.Services;
using Payments.Mvc.Models.InvoiceApiViewModels;
using Payments.Mvc.Services;

namespace Payments.Mvc.Controllers
{
    [Route("api/invoices")]
    public class InvoicesApiController : ApiController
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IStorageService _storageService;

        public InvoicesApiController(ApplicationDbContext dbContext, IInvoiceService invoiceService, IStorageService storageService) : base(dbContext)
        {
            _invoiceService = invoiceService;
            _storageService = storageService;
        }

        /// <summary>
        /// Fetch invoice details
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Invoice), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Invoice>> Get(int id)
        {
            var team = await GetAuthorizedTeam();

            var invoice = await _dbContext.Invoices
                .Include(i => i.Attachments)
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id && i.Team.Id == team.Id);

            if (invoice == null)
            {
                return NotFound(new { });
            }

            return invoice;
        }
        
        /// <summary>
        /// Create invoice
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(CreateInvoiceResult), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceModel model)
        {
            var team = await GetAuthorizedTeam();

            // validate model
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(new
                {
                    success = false,
                    errorMessage = "Errors found in request",
                    modelState = ModelState
                });
            }

            // create invoices
            IReadOnlyList<Invoice> invoices;
            try
            {
                invoices = await _invoiceService.CreateInvoices(model, team);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(ex.ParamName, ex.Message);
                return new BadRequestObjectResult(new
                {
                    success = false,
                    errorMessage = "Errors found in request",
                    modelState = ModelState
                });
            }

            // record action on invoices
            foreach (var invoice in invoices)
            {
                var action = new History()
                {
                    Type = HistoryActionTypes.InvoiceCreated.TypeCode,
                    ActionDateTime = DateTime.UtcNow,
                };
                invoice.History.Add(action);
            }

            await _dbContext.SaveChangesAsync();

            return new JsonResult(new CreateInvoiceResult
            {
                Success = true,
                Ids = invoices.Select(i => i.Id).ToArray(),
            });
        }

        /// <summary>
        /// Edit invoice
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("{id}")]
        [ProducesResponseType(typeof(EditInvoiceResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Edit(int id, [FromBody] EditInvoiceModel model)
        {
            var team = await GetAuthorizedTeam();

            // find item
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Attachments)
                .Include(i => i.Team)
                .Where(i => i.Team.Id == team.Id)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // validate model
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(new
                {
                    success = false,
                    errorMessage = "Errors found in request",
                    modelState = ModelState
                });
            }

            try
            {
                await _invoiceService.UpdateInvoice(invoice, model);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(ex.ParamName, ex.Message);
                return new BadRequestObjectResult(new
                {
                    success = false,
                    errorMessage = "Errors found in request",
                    modelState = ModelState
                });
            }

            // record action
            var action = new History()
            {
                Type = HistoryActionTypes.InvoiceEdited.TypeCode,
                ActionDateTime = DateTime.UtcNow,
            };
            invoice.History.Add(action);

            await _dbContext.SaveChangesAsync();

            // build response, send possible new link
            var response = new EditInvoiceResult
            {
                Success = true,
                Id = invoice.Id,
            };
            if (invoice.Sent)
            {
                response.PaymentLink = Url.Action("Pay", "Payments", new { id = invoice.LinkId });
            }

            return new JsonResult(response);
        }

        /// <summary>
        /// Send invoice
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("{id}/send")]
        [ProducesResponseType(typeof(SendInvoiceResult), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Send(int id, [FromBody]SendInvoiceModel model)
        {
            var team = await GetAuthorizedTeam();

            // find item
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Team)
                .Include(i => i.Coupon)
                .Where(i => i.Team.Id == team.Id)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound(new
                {
                    success = false,
                    errorMessage = "Invoice Not Found",
                });
            }

            if (model == null)
            {
                model = new SendInvoiceModel();
            }

            await _invoiceService.SendInvoice(invoice, model);

            // record action
            var action = new History()
            {
                Type = HistoryActionTypes.InvoiceSent.TypeCode,
                ActionDateTime = DateTime.UtcNow,
            };
            invoice.History.Add(action);

            await _dbContext.SaveChangesAsync();

            return new JsonResult(new EditInvoiceResult
            {
                Success = true,
                Id = invoice.Id,
                PaymentLink = Url.Action("Pay", "Payments", new { id = invoice.LinkId }),
            });
        }

        [HttpPost("{id}/file")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddAttachment(int id, IFormFile file)
        {
            var team = await GetAuthorizedTeam();

            // find item
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Attachments)
                .Include(i => i.Team)
                .Where(i => i.Team.Id == team.Id)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // upload file
            var identifier = await _storageService.UploadAttachment(file);

            // add to invoice
            var attachment = new InvoiceAttachment()
            {
                Invoice     = invoice,
                Identifier  = identifier,
                FileName    = file.FileName,
                ContentType = file.ContentType,
                Size        = file.Length,
            };

            invoice.Attachments.Add(attachment);
            await _dbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                identifier,
            });
        }
    }
}
