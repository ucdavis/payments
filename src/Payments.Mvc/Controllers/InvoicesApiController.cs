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
                .Include(i => i.RechargeAccounts)
                .FirstOrDefaultAsync(i => i.Id == id && i.Team.Id == team.Id);

            if (invoice == null)
            {
                return NotFound(new { });
            }

            return invoice;
        }


        /// <summary>
        /// Fetch the minimal invoice details needed by API clients.
        /// </summary>
        /// <param name="id">Invoice identifier.</param>
        /// <returns>A simplified invoice response.</returns>
        [HttpGet("{id}/simple")]
        [ProducesResponseType(typeof(SimpleInvoiceResult), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<SimpleInvoiceResult>> GetSimple(int id)
        {
            var team = await GetAuthorizedTeam();

            var invoice = await _dbContext.Invoices
                .AsNoTracking()
                .Where(i => i.Id == id && i.Team.Id == team.Id)
                .Select(i => new SimpleInvoiceResult
                {
                    Id = i.Id,
                    Status = i.Status,
                    Type = i.Type,
                    LinkId = i.LinkId,
                    CustomerEmail = i.CustomerEmail,
                    TotalAmount = i.CalculatedTotal.ToString("F2"),
                    ExternalIdentifier = i.ExternalIdentifier,
                    ExternalId = i.ExternalId,
                    ExternalLink = i.ExternalLink
                })
                .FirstOrDefaultAsync();

            if (invoice == null)
            {
                return NotFound(new { });
            }

            return invoice;
        }

        [HttpGet("external/{externalIdentifier}/{externalId}")]
        [ProducesResponseType(typeof(List<SimpleInvoiceResult>), 200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<SimpleInvoiceResult>>> GetByExternal(string externalIdentifier, string externalId)
        {
            if (string.IsNullOrWhiteSpace(externalIdentifier) || string.IsNullOrWhiteSpace(externalId))
            {
                return BadRequest("Both externalIdentifier and externalId are required.");
            }

            var team = await GetAuthorizedTeam();

            var invoices = await _dbContext.Invoices
                .AsNoTracking()
                .Where(i => i.Team.Id == team.Id && i.ExternalIdentifier == externalIdentifier && i.ExternalId == externalId)
                .Select(i => new SimpleInvoiceResult
                {
                    Id = i.Id,
                    Status = i.Status,
                    Type = i.Type,
                    LinkId = i.LinkId,
                    CustomerEmail = i.CustomerEmail,
                    TotalAmount = i.CalculatedTotal.ToString("F2"),
                    ExternalIdentifier = i.ExternalIdentifier,
                    ExternalId = i.ExternalId,
                    ExternalLink = i.ExternalLink
                })
                .ToListAsync();

            if (!invoices.Any())
            {
                return NotFound(new { });
            }

            return invoices;
        }

        /// <summary>
        /// Fetch minimal invoice details for multiple external IDs.
        /// </summary>
        /// <param name="model">The external system identifier and external invoice IDs to retrieve.</param>
        /// <returns>Simplified invoice responses matching the supplied external IDs.</returns>
        [HttpPost("external")]
        [ProducesResponseType(typeof(SimpleInvoiceResult[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SimpleInvoiceResult[]>> GetByExternal(
            [FromBody] GetInvoicesByExternalIdsRequest model)
        {
            if (model == null ||
                string.IsNullOrWhiteSpace(model.ExternalIdentifier) ||
                model.ExternalIds == null ||
                model.ExternalIds.Length == 0 ||
                model.ExternalIds.Any(string.IsNullOrWhiteSpace))
            {
                return BadRequest("externalIdentifier and at least one externalId are required.");
            }

            var team = await GetAuthorizedTeam();
            var externalIds = model.ExternalIds.Distinct().ToArray();

            var invoices = await _dbContext.Invoices
                .AsNoTracking()
                .Where(i => i.Team.Id == team.Id &&
                            i.ExternalIdentifier == model.ExternalIdentifier &&
                            externalIds.Contains(i.ExternalId))
                .Select(i => new SimpleInvoiceResult
                {
                    Id = i.Id,
                    Status = i.Status,
                    Type = i.Type,
                    LinkId = i.LinkId,
                    CustomerEmail = i.CustomerEmail,
                    TotalAmount = i.CalculatedTotal.ToString("F2"),
                    ExternalIdentifier = i.ExternalIdentifier,
                    ExternalId = i.ExternalId,
                    ExternalLink = i.ExternalLink
                })
                .ToArrayAsync();

            if (invoices.Length == 0)
            {
                return NotFound(new { });
            }

            return invoices;
        }

        /// <summary>
        /// Mark Invoice as Deleted.
        /// Invoice must be in the drafted state or not paid or refunded
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Invoice</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Invoice>> Delete(int id)
        {
            var team = await GetAuthorizedTeam();
            var invoice = await _dbContext.Invoices
                .Where(i => i.Team.Id == team.Id)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            if (invoice.Deleted || invoice.Status == Invoice.StatusCodes.Deleted)
            {
                return BadRequest(new { errorMessage = "Invoice already deleted." });
            }

            if (invoice.Status != Invoice.StatusCodes.Draft)
            {
                if (invoice.Paid || invoice.Refunded)
                {
                    return BadRequest(new { errorMessage = "Invoice not in a state that can be deleted." });
                }
            }

            // mark as deleted
            invoice.Status = Invoice.StatusCodes.Deleted;
            invoice.Deleted = true;
            invoice.DeletedAt = DateTime.UtcNow;

            // remove links
            invoice.Sent = false;
            invoice.SentAt = null;
            invoice.LinkId = null;

            //record action
            var action = new History()
            {
                Type = HistoryActionTypes.InvoiceDeleted.TypeCode,
                ActionDateTime = DateTime.UtcNow,
                Actor = "API",
            };
            invoice.History.Add(action);

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Create one or more invoices
        /// </summary>
        /// <remarks>
        /// A separate invoice is created for each customer in the request.
        ///
        /// Invoice type must be CC (credit card) or Recharge. For a CC invoice,
        /// accountId must identify an account belonging to the authorized team. When
        /// useDefaultAccount is true, the team's active default account is used only
        /// when the supplied accountId is not found; it does not replace a valid account.
        /// </remarks>
        /// <param name="model">Invoice details.</param>
        /// <returns>The identifiers of the created invoices.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(CreateInvoiceResult), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceModel model)
        {
            var team = await GetAuthorizedTeam();

            if (string.IsNullOrWhiteSpace(model.Type))
            {
                model.Type = Invoice.InvoiceTypes.CreditCard;
            }

            if (model.Type == Invoice.InvoiceTypes.CreditCard)
            {
                if (team.AllowedInvoiceType == Team.AllowedInvoiceTypes.Recharge)
                {
                    ModelState.AddModelError("Type", "This team is not allowed to create credit card invoices.");
                }
            }
            if (model.Type == Invoice.InvoiceTypes.Recharge)
            {
                if (team.AllowedInvoiceType == Team.AllowedInvoiceTypes.CreditCard)
                {
                    ModelState.AddModelError("Type", "This team is not allowed to create recharge invoices.");
                }
                foreach (var rechargeAcct in model.RechargeAccounts)
                {
                    rechargeAcct.EnteredByKerb = "API";
                    rechargeAcct.EnteredByName = "API";
                }
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
                .Include(i => i.RechargeAccounts)
                .Where(i => i.Team.Id == team.Id)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            if (invoice.Type == Invoice.InvoiceTypes.Recharge)
            {
                foreach(var rechargeAcct in model.RechargeAccounts.Where(a => a.Id == 0))
                {
                    rechargeAcct.EnteredByKerb = "API";
                    rechargeAcct.EnteredByName = "API";
                }
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
                .Include(i => i.RechargeAccounts)
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
