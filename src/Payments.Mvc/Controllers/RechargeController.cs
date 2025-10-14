using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
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
        /// <exception cref="System.NotImplementedException"></exception>
        [HttpGet]
        public async Task<IActionResult> Pay(string id)
        {
            //var user = await _userManager.GetUserAsync(User); //Might only need this on the post action

            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Team)
                .Include(i => i.Attachments)
                .FirstOrDefaultAsync(i => i.LinkId == id);

            //if (invoice == null)
            //{
            //    // check expired link id
            //    var link = await _dbContext.InvoiceLinks
            //        .Include(l => l.Invoice)
            //        .ThenInclude(i => i.Team)
            //        .FirstOrDefaultAsync(l => l.LinkId == id);

            //    // still not found
            //    if (link == null)
            //    {
            //        return PublicNotFound();
            //    }

            //    // if the invoice has a new link id,
            //    // just forward them to the corrected invoice
            //    if (!string.IsNullOrWhiteSpace(link.Invoice.LinkId))
            //    {
            //        Message = "Your link was expired/old. We've forwarded you to the new link. Please review the invoice for any changes before proceeding.";
            //        return RedirectToAction("Pay", new { id = link.Invoice.LinkId });
            //    }

            //    // otherwise, the invoice is probably back in draft
            //    var expiredModel = new ExpiredInvoiceViewModel()
            //    {
            //        Team = new PaymentInvoiceTeamViewModel(link.Invoice.Team)
            //    };
            //    return View("Expired", expiredModel);
            //}

            //// the customer isn't allowed access to draft or cancelled invoices
            //if (invoice.Status == Invoice.StatusCodes.Draft || invoice.Status == Invoice.StatusCodes.Cancelled)
            //{
            //    return PublicNotFound();
            //}

            //invoice.UpdateCalculatedValues();

            //var model = CreateInvoicePaymentViewModel(invoice);

            //if (invoice.Status == Invoice.StatusCodes.Sent)
            //{
            //    // prepare dictionary
            //    var dictionary = invoice.GetPaymentDictionary();
            //    dictionary.Add("access_key", _cyberSourceSettings.AccessKey);
            //    dictionary.Add("profile_id", _cyberSourceSettings.ProfileId);

            //    var fieldNames = string.Join(",", dictionary.Keys);
            //    dictionary.Add("signed_field_names", "signed_field_names," + fieldNames);

            //    ViewBag.Signature = _dataSigningService.Sign(dictionary);

            //    ViewBag.CyberSourceUrl = _cyberSourceSettings.BaseUrl;

            //    model.PaymentDictionary = dictionary;
            //}

            //return View(model);



            throw new System.NotImplementedException();
        }
    }
}
