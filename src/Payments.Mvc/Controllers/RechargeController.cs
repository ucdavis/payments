using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Extensions;
using Payments.Core.Services;
using Payments.Mvc.Identity;
using Payments.Mvc.Models.Configuration;
using Payments.Mvc.Models.PaymentViewModels;
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

        public RechargeController(ApplicationDbContext dbContext, IAggieEnterpriseService aggieEnterpriseService, ApplicationUserManager userManager)
        {
            _dbContext = dbContext;
            _aggieEnterpriseService = aggieEnterpriseService;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("api/recharge/validate")]
        public async Task<IActionResult> ValidateChartString(string chartString, CreditDebit direction)
        {
            var result = await _aggieEnterpriseService.IsRechargeAccountValid(chartString, direction);

            //This may not need to return the entire result object

            return new JsonResult(result);
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

            //var model = CreateRechargePaymentViewModel(invoice);

            if (invoice.Status == Invoice.StatusCodes.Sent)
            {
                //This is valid status to pay, but I think we want to allow the other statuses through so it can be viewed, but not edited.
            }

            ViewBag.Id = id;

            return View(model);

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
                DebitRechargeAccounts = invoice.RechargeAccounts.Where(ra => ra.Direction == RechargeAccount.CreditDebit.Debit).ToList()
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

            // For now, return an error with the updated model until the full implementation is done
            //var returnModel = CreateRechargeInvoiceViewModel(invoice);
            ////returnModel.ErrorMessage = "Your payment information could not be updated.";
            ////return BadRequest(returnModel);

            //invoice.Status = Invoice.StatusCodes.PendingApproval;
            //_dbContext.Invoices.Update(invoice);
            //await _dbContext.SaveChangesAsync();

            //return Ok();


            throw new System.NotImplementedException();


            //// the customer isn't allowed access to draft or cancelled invoices
            if (invoice.Status == Invoice.StatusCodes.Draft || invoice.Status == Invoice.StatusCodes.Cancelled)
            {
                return PublicNotFound();
            }
            if (invoice.Status != Invoice.StatusCodes.Sent)
            {
                return RedirectToAction("Pay", new { id = invoice.LinkId });
            }

            //// remove any existing debit recharge accounts
            //_dbContext.RechargeAccounts.RemoveRange(invoice.RechargeAccounts.Where(ra => ra.Direction == RechargeAccount.CreditDebit.Debit));
            //// add any new debit recharge accounts
            //if (model.DebitRechargeAccounts != null)
            //{
            //    foreach (var ra in model.DebitRechargeAccounts)
            //    {
            //        // only add valid ones
            //        if (!string.IsNullOrWhiteSpace(ra.ChartString) && !string.IsNullOrWhiteSpace(ra.PurchaseOrderNumber))
            //        {
            //            ra.Id = 0;
            //            ra.Direction = RechargeAccount.CreditDebit.Debit;
            //            ra.InvoiceId = invoice.Id;
            //            _dbContext.RechargeAccounts.Add(ra);
            //        }
            //    }
            //}
            //await _dbContext.SaveChangesAsync();
            //Message = "Your payment information has been updated.";
            return RedirectToAction("Pay", new { id = invoice.LinkId });
        }

        public ActionResult PublicNotFound()
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

    }
}
