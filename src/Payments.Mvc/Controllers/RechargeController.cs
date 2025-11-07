using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Extensions;
using Payments.Core.Models.History;
using Payments.Core.Models.Invoice;
using Payments.Core.Models.Validation;
using Payments.Core.Services;
using Payments.Mvc.Identity;
using Payments.Mvc.Models.PaymentViewModels;
using Payments.Mvc.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Payments.Core.Domain.RechargeAccount;

namespace Payments.Mvc.Controllers
{
    public class RechargeController : SuperController
    {
        private readonly ApplicationDbContext _dbContext;
        private IAggieEnterpriseService _aggieEnterpriseService;
        private readonly ApplicationUserManager _userManager;
        private readonly IInvoiceService _invoiceService;

        public RechargeController(ApplicationDbContext dbContext, IAggieEnterpriseService aggieEnterpriseService, ApplicationUserManager userManager, IInvoiceService invoiceService)
        {
            _dbContext = dbContext;
            _aggieEnterpriseService = aggieEnterpriseService;
            _userManager = userManager;
            _invoiceService = invoiceService;
        }

        [HttpGet]
        [Route("api/recharge/validate")]
        public async Task<IActionResult> ValidateChartString(string chartString, CreditDebit direction, bool checkApprover = false)
        {
            var result = await _aggieEnterpriseService.IsRechargeAccountValid(chartString, direction);

            //This may not need to return the entire result object


            //check approver probably will only be used with debits
            if(checkApprover)
            {
                var user = await _userManager.GetUserAsync(User);
                if(user != null && result.Approvers != null && result.Approvers.Count > 0)
                {
                    if(!result.Approvers.Any(a => string.Equals(a.Email, user.Email, StringComparison.OrdinalIgnoreCase)))
                    {
                        result.IsValid = false;
                        result.Messages.Add("You are not an approver for this chart string.");
                    }
                }
            }

            return new JsonResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> Preview(int id)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Team)
                .Include(i => i.Attachments)
                .Include(i => i.RechargeAccounts.Where(ra => ra.Direction == RechargeAccount.CreditDebit.Debit))
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return PublicNotFound();
            }


            invoice.UpdateCalculatedValues();

            var model = CreateRechargeInvoiceViewModel(invoice);

            //var model = CreateRechargePaymentViewModel(invoice);

            if (invoice.Status != Invoice.StatusCodes.Sent)
            {
                //This is valid status to pay, but I think we want to allow the other statuses through so it can be viewed, but not edited.
            }

            ViewBag.Id = id;

            return View(model);
        }


        /// <summary>
        /// Pay will be the action where the customer who was emailed the invoice (or CC'd on it) can go in and enter or edit any of the debit recharge information
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Pay(string id)
        {
            //var user = await _userManager.GetUserAsync(User); //Might only need this on the post action

            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Team)
                .Include(i => i.Attachments)
                .Include(i => i.RechargeAccounts.Where(ra => ra.Direction == RechargeAccount.CreditDebit.Debit))
                .FirstOrDefaultAsync(i => i.LinkId == id);

            if (invoice == null)
            {
                // check expired link id
                var link = await _dbContext.InvoiceLinks
                    .Include(l => l.Invoice)
                    .ThenInclude(i => i.Team)
                    .FirstOrDefaultAsync(l => l.LinkId == id);

                // still not found
                if (link == null)
                {
                    return PublicNotFound();
                }

                //TODO: Will this work? It "Should" since it is the Get.
                // if the invoice has a new link id,
                // just forward them to the corrected invoice
                if (!string.IsNullOrWhiteSpace(link.Invoice.LinkId))
                {
                    Message = "Your link was expired/old. We've forwarded you to the new link. Please review the invoice for any changes before proceeding.";
                    return RedirectToAction("Pay", new { id = link.Invoice.LinkId });
                }

                // otherwise, the invoice is probably back in draft
                var expiredModel = new ExpiredInvoiceViewModel()
                {
                    Team = new PaymentInvoiceTeamViewModel(link.Invoice.Team)
                };
                return View("Expired", expiredModel);
            }

            // the customer isn't allowed access to draft or cancelled invoices
            if (invoice.Status == Invoice.StatusCodes.Draft || invoice.Status == Invoice.StatusCodes.Cancelled)
            {
                return PublicNotFound();
            }

            invoice.UpdateCalculatedValues();

            var model = CreateRechargeInvoiceViewModel(invoice);

            if (invoice.Status == Invoice.StatusCodes.Sent)
            {
                //This is valid status to pay, but I think we want to allow the other statuses through so it can be viewed, but not edited.
            }

            ViewBag.Id = id;

            return View(model);

        }



        private static RechargeInvoiceViewModel CreateRechargeInvoiceViewModel(Invoice invoice)
        {
            var model = new RechargeInvoiceViewModel()
            {
                Id = invoice.GetFormattedId(),
                LinkId = invoice.LinkId,
                //CustomerName = invoice.CustomerName,
                //CustomerCompany = invoice.CustomerCompany,
                //CustomerEmail = invoice.CustomerEmail,
                //CustomerAddress = invoice.CustomerAddress,
                Memo = invoice.Memo,
                Items = invoice.Items,
                Attachments = invoice.Attachments,
                //Coupon = invoice.Coupon,
                //Discount = invoice.CalculatedDiscount,
                //TaxAmount = invoice.CalculatedTaxAmount,
                //TaxPercent = invoice.TaxPercent,
                Subtotal = invoice.CalculatedSubtotal,
                Total = invoice.CalculatedTotal,
                DueDate = invoice.DueDate,
                Paid = invoice.Paid,
                PaidDate = invoice.PaidAt.ToPacificTime(),
                Team = new PaymentInvoiceTeamViewModel(invoice.Team),
                Status = invoice.Status,
                DebitRechargeAccounts = invoice.RechargeAccounts.Where(ra => ra.Direction == RechargeAccount.CreditDebit.Debit).ToList(),
            };
            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Link Id</param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Pay(string id, [FromBody] RechargeAccount[] model)
        {

            var user = await _userManager.GetUserAsync(User);
            //This need to update the status, validate the chartStrings, write to the history, send emails, etc. (We probably also want the invoice details page to be able to resend the email(s) for approvals

            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Team)
                .Include(i => i.Attachments)
                .Include(i => i.RechargeAccounts.Where(ra => ra.Direction == RechargeAccount.CreditDebit.Debit))
                .FirstOrDefaultAsync(i => i.LinkId == id);
            if (invoice == null)
            {
                return NotFound(new { message = "Invoice not found." });
            }



            //// the customer isn't allowed access to draft or cancelled invoices
            if (invoice.Status == Invoice.StatusCodes.Draft || invoice.Status == Invoice.StatusCodes.Cancelled)
            {
                return NotFound(new { message = "Invoice not found in correct Status." });
            }
            if (invoice.Status != Invoice.StatusCodes.Sent)
            {
                //This is ok here, this is the post and it isn't valid. It IS valid in the Get action above.
                return BadRequest("Invoice is not in a valid status for payment. Please refresh the page.");
            }

            // validation:
            if (model.Sum(ra => ra.Amount) != invoice.CalculatedTotal)
            {
                return BadRequest("The total of the recharge accounts does not match the invoice total. Please review and try again.");
            }

            var savedApprovers = new List<Approver>();
            foreach (var item in model)
            {
                var validationResult = await _aggieEnterpriseService.IsRechargeAccountValid(item.FinancialSegmentString, RechargeAccount.CreditDebit.Debit);
                if (!validationResult.IsValid)
                {
                    return BadRequest($"The chart string {item.FinancialSegmentString} is not valid: {validationResult.Message}");
                }
                if (item.Amount <= 0)
                {
                    return BadRequest($"The amount for chart string {item.FinancialSegmentString} must be greater than zero.");
                }
                if (item.FinancialSegmentString != validationResult.ChartString)
                {
                    item.FinancialSegmentString = validationResult.ChartString; //Just in case the natural account was replaced
                }
                savedApprovers.AddRange(validationResult.Approvers);
            }

            savedApprovers = savedApprovers.DistinctBy(a => a.Email?.ToLower()).ToList(); //The model will have the complete list of debits, so we don't need to check existing ones.

            //Ok, we have got this far, so everything is valid, so we can save the recharge accounts
            // We need to check if they were changed.

            //We need to remove any that were deleted. But ONLY debit ones
            var toRemove = new List<RechargeAccount>();
            foreach (var existing in invoice.RechargeAccounts.Where(a => a.Direction == CreditDebit.Debit))
            {
                var found = model.FirstOrDefault(a => a.Id == existing.Id);
                if (found == null)
                {
                    if (existing.Direction != CreditDebit.Debit)
                    {
                        continue; //Should not happen due to the filter above, but just in case
                    }
                    toRemove.Add(existing);
                }
            }

            var rechargeAccountToAdd = new List<RechargeAccount>();

            foreach (var item in model)
            {
                var existing = invoice.RechargeAccounts.FirstOrDefault(a => a.Id == item.Id); //Could filter out when id == 0
                if (existing != null)
                {
                    if (existing.FinancialSegmentString != item.FinancialSegmentString || existing.Amount != item.Amount || existing.Notes != item.Notes)
                    {
                        //If nothing changed, we don't update who entered it.
                        ////Update amount
                        existing.FinancialSegmentString = item.FinancialSegmentString;
                        existing.Amount = item.Amount;
                        existing.EnteredByKerb = user.CampusKerberos;
                        existing.EnteredByName = user.Name;
                        existing.Notes = item.Notes;
                        _dbContext.RechargeAccounts.Update(existing); //Probably not needed since we are tracking it, but just in case
                    }
                }
                else
                {
                    ////New one (can't directly add to invoice.RechargeAccounts because then it finds it above) could filter where id != 0 but this is clearer
                    rechargeAccountToAdd.Add(new RechargeAccount()
                    {
                        Direction = CreditDebit.Debit,
                        FinancialSegmentString = item.FinancialSegmentString,
                        Amount = item.Amount,
                        InvoiceId = invoice.Id,
                        EnteredByKerb = user.CampusKerberos,
                        EnteredByName = user.Name,
                        Percentage = item.Percentage,
                        Notes = item.Notes
                    });
                }
            }

            if(rechargeAccountToAdd.Count > 0)
            {
                foreach(var ra in rechargeAccountToAdd)
                {
                    invoice.RechargeAccounts.Add(ra);
                }
            }

            //Now remove the deleted ones
            if (toRemove.Count > 0)
            {
                _dbContext.RechargeAccounts.RemoveRange(toRemove);
            }

            var action = new History()
            {
                Type = HistoryActionTypes.RechargePaidByCustomer.TypeCode,
                ActionDateTime = DateTime.UtcNow,
                Actor = user.Name,
                Data = new RechargePaidByCustomerHistoryActionType().SerializeData(new RechargePaidByCustomerHistoryActionType.DataType
                {
                    RechargeAccounts = model
                })
            };
            invoice.History.Add(action);

            if(invoice.RechargeAccounts.Where(ra => ra.Direction == CreditDebit.Debit).Sum(a => a.Amount) != invoice.CalculatedTotal)
            {
                return BadRequest("The total of the recharge accounts does not match the invoice total after saving. Please review and try again.");
            }


            await _dbContext.SaveChangesAsync(); //Maybe wait for all changes?



            //Need to notify the approvers. This will require a new email template.

            invoice.PaidAt = DateTime.UtcNow;
            //TODO: Also set the paid flag? Or.... probably better, wait for the financial approve step. Then I can use the PaidAt to determine auto pay, and the Paid flag for the PaymentController download (receipt) 

            invoice.Status = Invoice.StatusCodes.PendingApproval;

            var emails = new List<EmailRecipient>();
            foreach (var approver in savedApprovers)
            {
                emails.Add(new EmailRecipient()
                {
                    Email = approver.Email,
                    Name = approver.Name
                });
            }

            await _invoiceService.SendFinancialApproverEmail(invoice, new SendApprovalModel()
            {
                emails = emails.ToArray(),
                bccEmails = "" //TODO: Add any BCC emails if needed
            });

            var notificationAction = new History()
            {
                Type = HistoryActionTypes.RechargeSentToFinancialApprovers.TypeCode,
                ActionDateTime = DateTime.UtcNow,
                Actor = "System",
                Data = new RechargeSentToFinancialApproversHistoryActionType().SerializeData(new RechargeSentToFinancialApproversHistoryActionType.DataType
                {
                    FinancialApprovers = savedApprovers.Select(a => new RechargeSentToFinancialApproversHistoryActionType.FinancialApprover()
                    {
                        Name = a.Name,
                        Email = a.Email
                    }).ToArray()
                })
            };

            invoice.History.Add(notificationAction);

            //_dbContext.Invoices.Update(invoice);
            await _dbContext.SaveChangesAsync();


            return Ok();

        }


        [HttpGet]
        public async Task<IActionResult> FinancialApprove(string id)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Team)
                .Include(i => i.Attachments)
                .Include(i => i.RechargeAccounts.Where(ra => ra.Direction == RechargeAccount.CreditDebit.Debit))
                .FirstOrDefaultAsync(i => i.LinkId == id);
            //I think his is ok.
            if (invoice == null)
            {
                return PublicNotFound();
            }

            if (invoice.Status == Invoice.StatusCodes.Draft || invoice.Status == Invoice.StatusCodes.Cancelled || invoice.Status == Invoice.StatusCodes.Sent)
            {
                return PublicNotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return PublicNotFound();
            }


            var (isApprover, canApprove, actionableRechargeAccounts, displayOnlyRechargeAccounts) = await GetApproverRechargeAccounts(invoice, user);

            if (invoice.Status != Invoice.StatusCodes.PendingApproval || !isApprover)
            {
                //We still want to show this so they can view it, but not approve/edit/reject it.
                //We will pass a canEdit or similar flag to the view for that.
                isApprover = false;
                canApprove = false;
                actionableRechargeAccounts = new List<RechargeAccount>();
                displayOnlyRechargeAccounts = invoice.RechargeAccounts.Where(ra => ra.Direction == CreditDebit.Debit).ToList();
            }

            var model = CreateRechargeInvoiceViewModel(invoice);
            model.DisplayDebitRechargeAccounts = displayOnlyRechargeAccounts;
            model.DebitRechargeAccounts = actionableRechargeAccounts;
            model.CanApprove = canApprove;


            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> FinancialApprove(string id, string actionType, string rejectReason, [FromBody] RechargeAccount[] model)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Team)
                .Include(i => i.Attachments)
                .Include(i => i.RechargeAccounts.Where(ra => ra.Direction == RechargeAccount.CreditDebit.Debit))
                .FirstOrDefaultAsync(i => i.LinkId == id);

            if (invoice == null)
            {
                return NotFound(new { message = "Invoice not found." });
            }

            if (invoice.Status != Invoice.StatusCodes.PendingApproval)
            {
                return BadRequest("Invoice is not in a valid status for approval. Please refresh the page.");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var (isApprover, canApprove, actionableRechargeAccounts, displayOnlyRechargeAccounts) = await GetApproverRechargeAccounts(invoice, user);

            if (!canApprove)
            {
                //I'm using canApprove here because isApprover could be true, but they have already approved all their accounts.ad
                return BadRequest("You are not authorized to act on this invoice. Please refresh the page.");
            }

            if(actionType == "Reject")
            {
                if(string.IsNullOrWhiteSpace(rejectReason))
                {
                    return BadRequest("A reason for rejection must be provided.");
                }

                invoice.Status = Invoice.StatusCodes.Rejected;
                var actionEntry = new History()
                {
                    Type = HistoryActionTypes.RechargeRejectedByFinancialApprover.TypeCode,
                    ActionDateTime = DateTime.UtcNow,
                    Actor = $"{user.Name} ({user.Email})",
                    Data = rejectReason
                };
                invoice.History.Add(actionEntry);
                //_dbContext.Invoices.Update(invoice);
                await _dbContext.SaveChangesAsync();

                //Send rejection email?


                return Ok();
            }

            if(actionType != "Approve")
            {
                return BadRequest("Invalid action.");
            }

            //We are approving here.

            //Make sure no new recharge accounts were added or deleted form the list of actionable ones.
            if(model.Any(a => a.Id == 0))
            {
                return BadRequest("The recharge accounts may not be added. Please refresh the page and try again.");
            }
            foreach (var item in model)
            {
                var existing = actionableRechargeAccounts.FirstOrDefault(a => a.Id == item.Id);
                if (existing == null)
                {
                    return BadRequest("The recharge accounts may not be removed. Please refresh the page and try again.");
                }

                //We don't really care about checking if they changed the chart string, we will just pass whatever is in the model and validate/save that.
                var validationResult = await _aggieEnterpriseService.IsRechargeAccountValid(item.FinancialSegmentString, RechargeAccount.CreditDebit.Debit);
                if (!validationResult.IsValid)
                {
                    return BadRequest($"The chart string {item.FinancialSegmentString} is not valid: {validationResult.Message}");
                }
                if (!validationResult.Approvers.Any(a => a.Email == user.Email))
                {
                    return BadRequest($"You are not an approver for the chart string {item.FinancialSegmentString}.");
                }
                existing.FinancialSegmentString = validationResult.ChartString; //TODO: Make sure this is saved.
                existing.ApprovedByKerb = user.CampusKerberos;
                existing.ApprovedByName = user.Name;

            }

            if(invoice.RechargeAccounts.Where(ra => ra.Direction == CreditDebit.Debit).Any(ra => ra.ApprovedByKerb == null))
            {
                await _dbContext.SaveChangesAsync();
                //We don't need a history here bacause we show it on the table onthe details page.
                return Ok("Not all recharge accounts have been approved yet.");
            }

            //Ok, all recharge accounts have been approved.
            invoice.Paid = true;
            //invoice.PaidAt = DateTime.UtcNow; //Do we want to do this? We set it on the pay page....

            invoice.Status = Invoice.StatusCodes.Approved; //Or maybe a new status so the money movement job can pick it up?

            var approvalAction = new History()
            {
                Type = HistoryActionTypes.RechargeApprovedByFinancialApprover.TypeCode,
                ActionDateTime = DateTime.UtcNow,
                Data = "All debit recharge accounts have been approved."
            };

            invoice.History.Add(approvalAction);

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        public ActionResult PublicNotFound()
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        private async Task<(bool isApprover, bool canApprove, List<RechargeAccount> actionableRechargeAccounts, List<RechargeAccount> displayOnlyRechargeAccounts)> GetApproverRechargeAccounts(Invoice invoice, User user)
        {
            var isApprover = false; //We may want to distinguish between isApprover and canApprove
            var canApprove = false;
            var actionableRechargeAccounts = new List<RechargeAccount>();

            foreach (var ra in invoice.RechargeAccounts.Where(ra => ra.Direction == CreditDebit.Debit))
            {
                var validationResult = await _aggieEnterpriseService.IsRechargeAccountValid(ra.FinancialSegmentString, CreditDebit.Debit);
                if (validationResult.Approvers != null && validationResult.Approvers.Count > 0)
                {
                    if (validationResult.Approvers.Any(a => string.Equals(a.Email, user.Email, StringComparison.OrdinalIgnoreCase)))
                    {
                        isApprover = true;
                        if (ra.ApprovedByKerb == null && ra.EnteredByKerb != user.CampusKerberos) //If they entered it, they can't approve it.
                        {
                            canApprove = true;
                            actionableRechargeAccounts.Add(ra);
                        }
                    }
                }
            }

            var displayOnlyRechargeAccounts = invoice.RechargeAccounts.Where(ra => ra.Direction == CreditDebit.Debit).Except(actionableRechargeAccounts).ToList();

            return (isApprover, canApprove, actionableRechargeAccounts, displayOnlyRechargeAccounts);
        }

    }
}
