using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Models.History;
using Payments.Core.Services;
using Payments.Mvc.Helpers;
using Payments.Mvc.Identity;
using Payments.Mvc.Models.InvoiceViewModels;
using Payments.Mvc.Models.Roles;


namespace Payments.Mvc.Controllers
{
    [Authorize(Policy = PolicyCodes.TeamEditor)]
    public class InvoicesController : SuperController
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailService _emailService;

        public InvoicesController(ApplicationUserManager userManager, ApplicationDbContext dbContext, IEmailService emailService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _emailService = emailService;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {

            var query = _dbContext.Invoices
                .AsQueryable()
                .Where(i => i.Team.Slug == TeamSlug);

            // fetch filter from session
            var filter = GetInvoiceFilter();

            if (filter.Statuses.Any())
            {
                query = query.Where(i => filter.Statuses.Contains(i.Status));
            }

            if (filter.CreatedDateStart.HasValue)
            {
                query = query.Where(i => i.CreatedAt >= filter.CreatedDateStart.Value);
            }

            if (filter.CreatedDateEnd.HasValue)
            {
                query = query.Where(i => i.CreatedAt <= filter.CreatedDateEnd.Value);
            }

            var invoices = query
                .Take(100)
                .OrderByDescending(i => i.Id)
                .ToList();

            var model = new InvoiceListViewModel()
            {
                Invoices = invoices,
                Filter = filter
            };

            // setup dropdown viewmodels
            ViewBag.Statuses = Invoice.StatusCodes.GetAllCodes()
                .Select(c => new SelectListItem
                {
                    Text = c,
                    Value = c,
                });

            return View(model);
        }

        public IActionResult SetFilter(InvoiceFilterViewModel model)
        {
            // save filter to session
            SetInvoiceFilter(model);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.Attachments)
                .Include(i => i.Items)
                .Include(i => i.Payment)
                .Include(i => i.History)
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var team = await _dbContext.Teams
                .Include(t => t.Accounts)
                .FirstOrDefaultAsync(t => t.Slug == TeamSlug);

            ViewBag.Team = new { team.Id, team.Name, team.Slug };

            ViewBag.Accounts = team.Accounts
                .Where(a => a.IsActive)
                .Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.Description,
                    a.IsDefault,
                    a.Chart,
                    a.Account,
                    a.SubAccount,
                    a.Project,
                });

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // look for invoice
            var invoice = await _dbContext.Invoices
                .Include(i => i.Account)
                .Include(i => i.Items)
                .Include(i => i.Attachments)
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // fetch team data
            var team = await _dbContext.Teams
                .Include(t => t.Accounts)
                .FirstOrDefaultAsync(t => t.Slug == TeamSlug);

            ViewBag.Team = new { team.Id, team.Name, team.Slug };

            ViewBag.Accounts = team.Accounts
                .Where(a => a.IsActive)
                .Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.Description,
                    a.IsDefault,
                    a.Chart,
                    a.Account,
                    a.SubAccount,
                    a.Project,
                });

            // build model for view
            var model = new EditInvoiceViewModel()
            {
                AccountId  = invoice.Account.Id,
                Discount   = invoice.Discount,
                DueDate    = invoice.DueDate,
                TaxPercent = invoice.TaxPercent,
                Memo       = invoice.Memo,
                Customer   = new EditInvoiceCustomerViewModel()
                {
                    Name    = invoice.CustomerName,
                    Address = invoice.CustomerAddress,
                    Email   = invoice.CustomerEmail,
                },
                Items = invoice.Items.Select(i => new EditInvoiceItemViewModel()
                {
                    Id          = i.Id,
                    Amount      = i.Amount,
                    Description = i.Description,
                    Quantity    = i.Quantity,
                }).ToList(),
                Attachments = invoice.Attachments.Select(a => new EditInvoiceAttachmentViewModel()
                {
                    Id          = a.Id,
                    Identifier  = a.Identifier,
                    FileName    = a.FileName,
                    ContentType = a.ContentType,
                    Size        = a.Size,
                }).ToList()
            };

            // add other relevant data
            ViewBag.Id = id;
            ViewBag.Sent = invoice.Sent;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var team = await _dbContext.Teams.FirstOrDefaultAsync(t => t.Slug == TeamSlug);

            // find account
            var account = await _dbContext.FinancialAccounts
                .FirstOrDefaultAsync(a => a.Team.Slug == TeamSlug && a.Id == model.AccountId);

            if (account == null)
            {
                ModelState.AddModelError("AccountId", "Account Id not found for this team.");
            }
            else if (!account.IsActive)
            {
                ModelState.AddModelError("AccountId", "Account is inactive.");
            }

            // validate model
            if (!ModelState.IsValid)
            {
                return new JsonResult(new
                {
                    success = false,
                    errorMessage = "Errors found in request",
                    modelState = ModelState
                });
            }

            // manage multiple customer scenario
            var invoices = new List<Invoice>();
            foreach (var customer in model.Customers)
            {
                // create new object, track it
                var invoice = new Invoice
                {
                    Account         = account,
                    Team            = team,
                    Discount        = model.Discount,
                    TaxPercent      = model.TaxPercent,
                    DueDate         = model.DueDate,
                    CustomerAddress = customer.Address,
                    CustomerEmail   = customer.Email,
                    CustomerName    = customer.Name,
                    Memo            = model.Memo,
                    Status          = Invoice.StatusCodes.Draft,
                    Sent            = false,
                };

                // add line items
                var items = model.Items.Select(i => new LineItem()
                {
                    Amount      = i.Amount,
                    Description = i.Description,
                    Quantity    = i.Quantity,
                    Total       = i.Quantity * i.Amount,
                });
                invoice.Items = items.ToList();

                // add attachments
                var attachments = model.Attachments.Select(a => new InvoiceAttachment()
                {
                    Identifier  = a.Identifier,
                    FileName    = a.FileName,
                    ContentType = a.ContentType,
                    Size        = a.Size,
                });
                invoice.Attachments = attachments.ToList();

                // record action
                var action = new History()
                {
                    Type = HistoryActionTypes.InvoiceCreated.TypeCode,
                    ActionDateTime = DateTime.UtcNow,
                    Actor = user.Name,
                };
                invoice.History.Add(action);

                // start tracking for db
                invoice.UpdateCalculatedValues();
                _dbContext.Invoices.Add(invoice);

                invoices.Add(invoice);
            }

            _dbContext.SaveChanges();

            return new JsonResult(new
            {
                success = true,
                ids = invoices.Select(i => i.Id),
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromBody]EditInvoiceViewModel model)
        {
            // find item
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Attachments)
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // find account
            var account = await _dbContext.FinancialAccounts
                .FirstOrDefaultAsync(a => a.Team.Slug == TeamSlug && a.Id == model.AccountId);

            if (account == null)
            {
                ModelState.AddModelError("AccountId", "Account Id not found for this team.");
            }
            else if (!account.IsActive)
            {
                ModelState.AddModelError("AccountId", "Account is inactive.");
            }

            // validate model
            if (!ModelState.IsValid)
            {
                return new JsonResult(new
                {
                    success = false,
                    errorMessage = "Errors found in request",
                    modelState = ModelState
                });
            }
            // TODO: Consider modifying items instead of replacing
            // remove old items
            _dbContext.LineItems.RemoveRange(invoice.Items);

            // update invoice
            invoice.Account         = account;
            invoice.CustomerAddress = model.Customer.Address;
            invoice.CustomerEmail   = model.Customer.Email;
            invoice.CustomerName    = model.Customer.Name;
            invoice.Memo            = model.Memo;
            invoice.Discount        = model.Discount;
            invoice.TaxPercent      = model.TaxPercent;
            invoice.DueDate         = model.DueDate;

            // add line items
            var items = model.Items.Select(i => new LineItem()
            {
                Amount      = i.Amount,
                Description = i.Description,
                Quantity    = i.Quantity,
                Total       = i.Quantity * i.Amount,
            });
            invoice.Items = items.ToList();

            // add attachments
            var attachments = model.Attachments.Select(a => new InvoiceAttachment()
            {
                Identifier  = a.Identifier,
                FileName    = a.FileName,
                ContentType = a.ContentType,
                Size        = a.Size,
            });
            invoice.Attachments = attachments.ToList();

            // editing a sent invoice will modify the link id
            if (invoice.Sent)
            {
                SetInvoiceKey(invoice);
            }

            // record action
            var user = await _userManager.GetUserAsync(User);
            var action = new History()
            {
                Type = HistoryActionTypes.InvoiceEdited.TypeCode,
                ActionDateTime = DateTime.UtcNow,
                Actor = user.Name,
            };
            invoice.History.Add(action);

            // save to db
            invoice.UpdateCalculatedValues();
            _dbContext.SaveChanges();

            return new JsonResult(new
            {
                success = true
            });
        }

        [HttpPost]
        public async Task<IActionResult> Send(int id)
        {
            // find item
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound(new
                {
                    success = false,
                    errorMessage = "Invoice Not Found"
                });
            }

            if (invoice.Sent)
            {
                return BadRequest(new
                {
                    success = false,
                    errorMessage = "Invoice already sent."
                });
            }

            SetInvoiceKey(invoice);

            await _emailService.SendInvoice(invoice);

            invoice.Status = Invoice.StatusCodes.Sent;
            invoice.Sent = true;
            invoice.SentAt = DateTime.UtcNow;

            // record action
            var user = await _userManager.GetUserAsync(User);
            var action = new History()
            {
                Type = HistoryActionTypes.InvoiceSent.TypeCode,
                ActionDateTime = DateTime.UtcNow,
                Actor = user.Name,
            };
            invoice.History.Add(action);

            await _dbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true
            });
        }

        [HttpPost]
        public async Task<IActionResult> Unlock(int id)
        {
            // find item
            var invoice = await _dbContext.Invoices
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            if (invoice.Status != Invoice.StatusCodes.Sent)
            {
                return NotFound();
            }

            invoice.Status = Invoice.StatusCodes.Draft;
            invoice.Sent = false;
            invoice.SentAt = null;
            invoice.LinkId = null;

            // record action
            var user = await _userManager.GetUserAsync(User);
            var action = new History()
            {
                Type = HistoryActionTypes.InvoiceUnlocked.TypeCode,
                ActionDateTime = DateTime.UtcNow,
                Actor = user.Name,
            };
            invoice.History.Add(action);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Edit", "Invoices", new {id});
        }

        private void SetInvoiceKey(Invoice invoice)
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                // setup random 10 character key link id
                var linkId = InvoiceKeyHelper.GetUniqueKey();

                // look for duplicate
                if (_dbContext.Invoices.Any(i => i.LinkId == linkId)) continue;

                // set and exit
                invoice.LinkId = linkId;
                return;
            }

            throw new Exception("Failure to create new invoice link id in max attempts.");
        }

        private InvoiceFilterViewModel GetInvoiceFilter()
        {
            var filter = HttpContext.Session.GetObjectFromJson<InvoiceFilterViewModel>("InvoiceFilter");
            return filter ?? new InvoiceFilterViewModel();
        }

        private void SetInvoiceFilter(InvoiceFilterViewModel filter)
        {
            HttpContext.Session.SetObjectAsJson("InvoiceFilter", filter);
        }
    }
}
