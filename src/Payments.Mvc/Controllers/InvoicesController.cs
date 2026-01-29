using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Extensions;
using Payments.Core.Models.Configuration;
using Payments.Core.Models.History;
using Payments.Core.Models.Invoice;
using Payments.Core.Resources;
using Payments.Core.Services;
using Payments.Mvc.Helpers;
using Payments.Mvc.Identity;
using Payments.Mvc.Models.InvoiceViewModels;
using Payments.Mvc.Models.Roles;
using Payments.Mvc.Services;


namespace Payments.Mvc.Controllers
{
    [Authorize(Policy = PolicyCodes.TeamEditor)]
    public class InvoicesController : SuperController
    {
        private const int MaxRows = 1000;
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IInvoiceService _invoiceService;
        private readonly SlothSettings _slothSettings;

        public InvoicesController(ApplicationUserManager userManager, ApplicationDbContext dbContext, IInvoiceService invoiceService, IOptions<SlothSettings> slothSettings)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _invoiceService = invoiceService;
            _slothSettings = slothSettings.Value;
        }

        public IActionResult Index()
        {
            var query = _dbContext.Invoices
                .Where(i => i.Team.Slug == TeamSlug);

            // fetch filter from session
            var filter = GetInvoiceFilter();
            ViewBag.FilterApplied = false;

            // hide deleted unless explicitly asked to show
            if (!filter.ShowDeleted)
            {
                query = query.Where(i => !i.Deleted);
            }

            if (filter.Statuses.Any())
            {
                query = query.Where(i => filter.Statuses.Contains(i.Status));
                ViewBag.FilterApplied = true;
            }

            if (filter.CreatedDateStart.HasValue)
            {
                query = query.Where(i => i.CreatedAt >= filter.CreatedDateStart.FromPacificTime().Value);
                ViewBag.FilterApplied = true;
            }

            if (filter.CreatedDateEnd.HasValue)
            {
                query = query.Where(i => i.CreatedAt <= filter.CreatedDateEnd.FromPacificTime().Value);
                ViewBag.FilterApplied = true;
            }

            // get count for display reasons
            //var count = query.Count();

            var invoices = query
                .OrderByDescending(i => i.Id)
                .Take(MaxRows)
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

            if (invoices.Count >= MaxRows)
            {
                //Message = $"Only showing {MaxRows} records, adjust filters to show other invoices.";
                //Something strange with tempData. It shows up ONLY the second time
                ViewBag.Message = $"Only showing {MaxRows} records, adjust filters to show other invoices.";
            }
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
                .Include(i => i.Account)
                .Include(i => i.Coupon)
                .Include(i => i.Items)
                .Include(i => i.History)
                .Include(i => i.PaymentEvents)
                .Include(i => i.RechargeAccounts)
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // update totals
            invoice.UpdateCalculatedValues();

            ViewBag.TransactionLookup = $"{_slothSettings.TransactionLookup}{id}";
            if(invoice.Type == Invoice.InvoiceTypes.Recharge)
            {
                ViewBag.SlothDisbursementLookup = $"{_slothSettings.RechargeTransactionLookup}{id}";
            }

            return View(invoice);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var team = await _dbContext.Teams
                .Include(t => t.Accounts)
                .Include(t => t.Coupons)
                .FirstOrDefaultAsync(t => t.Slug == TeamSlug);

            ViewBag.Team = new { team.Id, team.Name, team.Slug, team.AllowedInvoiceType };

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
                    a.FinancialSegmentString,
                });

            ViewBag.Coupons = team.Coupons
                .Where(c => c.ExpiresAt == null || c.ExpiresAt >= DateTime.UtcNow.ToPacificTime().Date)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Code,
                    c.DiscountAmount,
                    c.DiscountPercent,
                    c.ExpiresAt,
                });

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // look for invoice
            var invoice = await _dbContext.Invoices
                .Include(i => i.Account)
                .Include(i => i.Coupon)
                .Include(i => i.Items)
                .Include(i => i.Attachments)
                .Include(i => i.RechargeAccounts)
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // fetch team data
            var team = await _dbContext.Teams
                .Include(t => t.Accounts)
                .Include(t => t.Coupons)
                .FirstOrDefaultAsync(t => t.Slug == TeamSlug);

            ViewBag.Team = new { team.Id, team.Name, team.Slug, team.ContactEmail, team.ContactPhoneNumber, team.AllowedInvoiceType };

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
                    a.FinancialSegmentString,
                });

            // include all active coupons, and the currently selected coupon
            ViewBag.Coupons = team.Coupons
                .Where(c => c.ExpiresAt == null
                        || c.ExpiresAt >= DateTime.UtcNow.ToPacificTime().Date
                        || c.Id == invoice.Coupon?.Id)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Code,
                    c.DiscountAmount,
                    c.DiscountPercent,
                    c.ExpiresAt,
                });

            // build model for view
            var model = new EditInvoiceModel()
            {
                AccountId = invoice.Account?.Id ?? 0,
                CouponId = invoice.Coupon?.Id ?? 0,
                ManualDiscount = invoice.ManualDiscount,
                DueDate = invoice.DueDate,
                TaxPercent = invoice.TaxPercent,
                Memo = invoice.Memo,
                Customer = new EditInvoiceCustomerModel()
                {
                    Name = invoice.CustomerName,
                    Address = invoice.CustomerAddress,
                    Email = invoice.CustomerEmail,
                    Company = invoice.CustomerCompany,
                },
                Items = invoice.Items.Select(i => new EditInvoiceItemModel()
                {
                    Amount = i.Amount,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    TaxExempt = i.TaxExempt,
                    Total = i.Amount * i.Quantity,
                }).ToList(),
                Attachments = invoice.Attachments.Select(a => new EditInvoiceAttachmentModel()
                {
                    Identifier = a.Identifier,
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    Size = a.Size,
                }).ToList(),
                RechargeAccounts = invoice.RechargeAccounts?.ToList() ?? new List<RechargeAccount>(),
                Type = invoice.Type,
            };

            // add other relevant data
            ViewBag.Id = id;
            ViewBag.Sent = invoice.Sent;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var team = await _dbContext.Teams.FirstOrDefaultAsync(t => t.Slug == TeamSlug);

            if(model.Type == Invoice.InvoiceTypes.CreditCard)
            {
                if(team.AllowedInvoiceType == Team.AllowedInvoiceTypes.Recharge)
                {
                    ModelState.AddModelError("Type", "This team is not allowed to create credit card invoices.");
                }
            }
            if(model.Type == Invoice.InvoiceTypes.Recharge)
            {
                if(team.AllowedInvoiceType == Team.AllowedInvoiceTypes.CreditCard)
                {
                    ModelState.AddModelError("Type", "This team is not allowed to create recharge invoices.");
                }
                foreach(var rechargeAcct in model.RechargeAccounts)
                {
                    rechargeAcct.EnteredByKerb = user.CampusKerberos;
                    rechargeAcct.EnteredByName = user.Name;
                }
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

            IReadOnlyList<Invoice> invoices;
            try
            {
                invoices = await _invoiceService.CreateInvoices(model, team);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(ex.ParamName, ex.Message);
                return new JsonResult(new
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
                    Actor = user.Name,
                };
                invoice.History.Add(action);
            }

            await _dbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                ids = invoices.Select(i => i.Id),
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromBody] EditInvoiceModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            // find item
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Attachments)
                .Include(i => i.Team)
                .Include(i => i.RechargeAccounts)
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            if(invoice.Type == Invoice.InvoiceTypes.Recharge)
            {
                foreach(var rechargeAcct in model.RechargeAccounts.Where(a => a.Id == 0))
                {                    
                    rechargeAcct.EnteredByKerb = user.CampusKerberos;
                    rechargeAcct.EnteredByName = user.Name;            
                }
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

            try
            {
                await _invoiceService.UpdateInvoice(invoice, model);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(ex.ParamName, ex.Message);
                return new JsonResult(new
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
                Actor = user.Name,
            };
            invoice.History.Add(action);

            // save to db
            _dbContext.SaveChanges();

            return new JsonResult(new
            {
                success = true
            });
        }

        [HttpPost]
        public async Task<IActionResult> Send(int id, [FromBody] SendInvoiceModel model)
        {
            // find item
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Team)
                .Include(i => i.Coupon)
                .Include(i => i.RechargeAccounts)
                .Where(i => i.Team.Slug == TeamSlug)
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

            var user = await _userManager.GetUserAsync(User);

            // check if the user email is the same as the customer email for recharges so it goes directly to the pending approval.
            if (invoice.Type == Invoice.InvoiceTypes.Recharge && 
                string.Equals(user.Email, invoice.CustomerEmail, StringComparison.OrdinalIgnoreCase) && 
                invoice.Status == Invoice.StatusCodes.Sent &&
                invoice.RechargeAccounts.Where(a => a.Direction == RechargeAccount.CreditDebit.Debit).Any())
            {
                invoice.PaidAt = DateTime.UtcNow;
                invoice.Status = Invoice.StatusCodes.PendingApproval;
                var approvalAction = new History()
                {
                    Type = HistoryActionTypes.RechargePaidByCustomer.TypeCode,
                    ActionDateTime = DateTime.UtcNow,
                    Actor = $"{user.Name} ({user.Email})",
                    Data = new RechargePaidByCustomerHistoryActionType().SerializeData(new RechargePaidByCustomerHistoryActionType.DataType
                    {
                        RechargeAccounts = invoice.RechargeAccounts.Where(a => a.Direction == RechargeAccount.CreditDebit.Debit).ToArray()
                    })
                };
                invoice.History.Add(approvalAction);
            }

            if (invoice.Type == Invoice.InvoiceTypes.Recharge && invoice.Status == Invoice.StatusCodes.PendingApproval)
            {
                //Need to resend these ones too
                var sentTo = await _invoiceService.SendFinancialApproverEmail(invoice, null); //Will pull them with a private method

                if (sentTo != null)
                {
                    var approvalSentAction = new History()
                    {
                        Type = HistoryActionTypes.RechargeSentToFinancialApprovers.TypeCode,
                        ActionDateTime = DateTime.UtcNow,
                        Actor = "System",
                        Data = new RechargeSentToFinancialApproversHistoryActionType().SerializeData(new RechargeSentToFinancialApproversHistoryActionType.DataType
                        {
                            FinancialApprovers = sentTo.emails.Select(a => new RechargeSentToFinancialApproversHistoryActionType.FinancialApprover()
                            {
                                Name = a.Name,
                                Email = a.Email
                            }).ToArray()
                        })
                    };
                    invoice.History.Add(approvalSentAction);
                }
            }

            // record action

            var action = new History()
            {
                Type = HistoryActionTypes.InvoiceSent.TypeCode,
                ActionDateTime = DateTime.UtcNow,
                Actor = user.Name,
            };
            if(invoice.Type == Invoice.InvoiceTypes.Recharge && invoice.RechargeAccounts.Where(a => a.Direction == RechargeAccount.CreditDebit.Debit).Any())
            {
                action.Data = new InvoiceSentHistoryActionType().SerializeData(new InvoiceSentHistoryActionType.DataType
                {
                    RechargeAccounts = invoice.RechargeAccounts.Where(a => a.Direction == RechargeAccount.CreditDebit.Debit).ToArray()
                });
            }
            invoice.History.Add(action);

            await _dbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
            });
        }

        [HttpPost]
        public async Task<IActionResult> Unlock(int id)
        {
            // find item
            var invoice = await _dbContext.Invoices
                .Include(a => a.RechargeAccounts)
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            //unlockable statuses
            var unlockableStatuses = new List<string>
            {
                Invoice.StatusCodes.Sent,
                Invoice.StatusCodes.Rejected,
                Invoice.StatusCodes.PendingApproval
            };

            if (!unlockableStatuses.Contains(invoice.Status))
            {
                return NotFound();
            }

            invoice.Status = Invoice.StatusCodes.Draft;
            invoice.Sent = false;
            invoice.SentAt = null;
            invoice.LinkId = null;

            if (invoice.Type == Invoice.InvoiceTypes.Recharge)
            {
                invoice.PaidAt = null;
                foreach (var acct in invoice.RechargeAccounts.Where(a => a.Direction == RechargeAccount.CreditDebit.Debit))
                {                
                    acct.ApprovedByKerb = null;
                    acct.ApprovedByName = null;
                }
            }

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

            return RedirectToAction("Edit", "Invoices", new { id });
        }

        [HttpPost]
        [Authorize(Policy = PolicyCodes.FinancialOfficer)]
        public async Task<IActionResult> MarkPaid(int id, string paidReason)
        {
            // find item
            var invoice = await _dbContext.Invoices
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // TODO: determine if this requirement is true
            // you can only mark a paid invoice if it's been sent
            if (invoice.Status != Invoice.StatusCodes.Sent)
            {
                return BadRequest();
            }

            // mark as complete with payment!
            invoice.Status = Invoice.StatusCodes.Completed;
            invoice.Paid = true;
            invoice.PaidAt = DateTime.UtcNow;
            invoice.PaymentType = PaymentTypes.Manual;

            // record action
            var user = await _userManager.GetUserAsync(User);
            var action = new History()
            {
                Type = HistoryActionTypes.MarkPaid.TypeCode,
                ActionDateTime = DateTime.UtcNow,
                Actor = user.Name,
                Data = paidReason,
            };
            invoice.History.Add(action);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Details", "Invoices", new { id });
        }

        [HttpPost]
        [Authorize(Roles = ApplicationRoleCodes.Admin)]
        public async Task<IActionResult> SetBackToPaid(int id)
        {
            // find item
            var invoice = await _dbContext.Invoices
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }


            if (invoice.Paid && invoice.Status != Invoice.StatusCodes.Processing)
            {
                return BadRequest();
            }

            if (invoice.PaidAt >= DateTime.UtcNow.AddDays(-5))
            {
                return BadRequest();
            }

            // mark as Paid so job will create sloth disbursements
            invoice.Status = Invoice.StatusCodes.Paid;

            // record action
            var user = await _userManager.GetUserAsync(User);
            var action = new History()
            {
                Type = HistoryActionTypes.SetBackToPaid.TypeCode,
                ActionDateTime = DateTime.UtcNow,
                Actor = user.Name,
            };
            invoice.History.Add(action);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Details", "Invoices", new { id });
        }

        [HttpPost]
        [Authorize(Roles = ApplicationRoleCodes.Admin)]
        public async Task<IActionResult> CancelRefundRequest(int id, string reason)
        {
            // find item
            var invoice = await _dbContext.Invoices
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }


            if (invoice.Paid && invoice.Status != Invoice.StatusCodes.Refunding)
            {
                return BadRequest();
            }


            // mark as Completed. //It is possible this was just paid and the disbursement never happened. So make a note of that in the UI
            invoice.Status = Invoice.StatusCodes.Completed;

            // record action
            var user = await _userManager.GetUserAsync(User);
            var action = new History()
            {
                Type = HistoryActionTypes.RefundCancelled.TypeCode,
                ActionDateTime = DateTime.UtcNow,
                Actor = user.Name,
                Data = reason,
            };
            invoice.History.Add(action);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Details", "Invoices", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> RequestRefund(int id, string refundReason)
        {
            // find item
            var invoice = await _dbContext.Invoices
                .Include(i => i.Team)
                .Include(i => i.PaymentEvents)
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // cant refund an unpaid invoice
            if (!invoice.Paid)
            {
                return NotFound();
            }

            // dont duplicate a refund request
            if (invoice.Status == Invoice.StatusCodes.Refunding)
            {
                return NotFound();
            }

            // cant refund an invoice already refunded
            if (invoice.Refunded)
            {
                return NotFound();
            }

            // get payment
            var payment = invoice.PaymentEvents.FirstOrDefault(p => p.ProcessorId == invoice.PaymentProcessorId);
            if (payment == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(User);
            // issue refund notice
            await _invoiceService.RefundInvoice(invoice, payment, refundReason, user);

            // record user action

            var action = new History()
            {
                Type = HistoryActionTypes.RefundRequested.TypeCode,
                Actor = user.Name,
                ActionDateTime = DateTime.UtcNow,
                Data = refundReason,
            };
            invoice.History.Add(action);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Details", "Invoices", new { id });
        }

        [HttpPost]
        [Authorize(Policy = PolicyCodes.FinancialOfficer)]
        public async Task<IActionResult> Refund(int id)
        {
            // find item
            var invoice = await _dbContext.Invoices
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            if (invoice.Status != Invoice.StatusCodes.Refunding)
            {
                return NotFound();
            }

            invoice.Status = Invoice.StatusCodes.Refunded;
            invoice.Refunded = true;
            invoice.RefundedAt = DateTime.UtcNow;

            // record user action
            var user = await _userManager.GetUserAsync(User);
            var action = new History()
            {
                Type = HistoryActionTypes.PaymentRefunded.TypeCode,
                Actor = user.Name,
                ActionDateTime = DateTime.UtcNow,
            };
            invoice.History.Add(action);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Details", "Invoices", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            // find item
            var invoice = await _dbContext.Invoices
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // you can only cancel invoices if they are sent, otherwise just delete it
            if (invoice.Status != Invoice.StatusCodes.Sent)
            {
                return NotFound();
            }

            // mark as cancelled and remove link
            invoice.Status = Invoice.StatusCodes.Cancelled;
            invoice.Sent = false;
            invoice.SentAt = null;
            invoice.LinkId = null;

            // record action
            var user = await _userManager.GetUserAsync(User);
            var action = new History()
            {
                Type = HistoryActionTypes.InvoiceCancelled.TypeCode,
                ActionDateTime = DateTime.UtcNow,
                Actor = user.Name,
            };
            invoice.History.Add(action);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Details", "Invoices", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // find item
            var invoice = await _dbContext.Invoices
                .Where(i => i.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // you can only delete invoices that are drafts
            if (invoice.Status != Invoice.StatusCodes.Draft)
            {
                if (invoice.Paid || invoice.Refunded)
                {
                    return NotFound();
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

            // record action
            var user = await _userManager.GetUserAsync(User);
            var action = new History()
            {
                Type = HistoryActionTypes.InvoiceDeleted.TypeCode,
                ActionDateTime = DateTime.UtcNow,
                Actor = user.Name,
            };
            invoice.History.Add(action);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Invoices");
        }

        [HttpPost]
        public IActionResult ClearFilter()
        {
            SetInvoiceFilter(null);
            return RedirectToAction("Index", "Invoices");
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
