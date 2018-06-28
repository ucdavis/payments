using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jsreport.AspNetCore;
using jsreport.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Mvc.Models.Configuration;
using Payments.Mvc.Models.CyberSource;
using Payments.Mvc.Models.PaymentViewModels;
using Payments.Mvc.Services;
using Serilog;

namespace Payments.Mvc.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly IDataSigningService _dataSigningService;
        private readonly ApplicationDbContext _dbContext;
        private readonly CyberSourceSettings _cyberSourceSettings;

        public PaymentsController(IDataSigningService dataSigningService, ApplicationDbContext dbContext, IOptions<CyberSourceSettings> cyberSourceSettings)
        {
            _dataSigningService = dataSigningService;
            _dbContext = dbContext;
            _cyberSourceSettings = cyberSourceSettings.Value;
        }

        [HttpGet]
        public async Task<ActionResult> Pay(string id)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Payment)
                .Include(i => i.Team)
                .FirstOrDefaultAsync(i => i.LinkId == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // the customer isn't allowed access to draft or cancelled invoices
            if (invoice.Status == Invoice.StatusCodes.Draft || invoice.Status == Invoice.StatusCodes.Cancelled)
            {
                return NotFound();
            }

            var model = CreateInvoicePaymentViewModel(invoice);

            if (invoice.Status == Invoice.StatusCodes.Sent)
            {
                // prepare dictionary
                var dictionary = invoice.GetPaymentDictionary();
                dictionary.Add("access_key", _cyberSourceSettings.AccessKey);
                dictionary.Add("profile_id", _cyberSourceSettings.ProfileId);

                var fieldNames = string.Join(",", dictionary.Keys);
                dictionary.Add("signed_field_names", "signed_field_names," + fieldNames);

                ViewBag.Signature = _dataSigningService.Sign(dictionary);

                ViewBag.CyberSourceUrl = _cyberSourceSettings.BaseUrl;

                model.PaymentDictionary = dictionary;
            }            

            return View(model);
        }

        private static InvoicePaymentViewModel CreateInvoicePaymentViewModel(Invoice invoice)
        {
            var model = new InvoicePaymentViewModel()
            {
                Id = invoice.Id.ToString(),
                LinkId = invoice.LinkId,
                CustomerName = invoice.CustomerName,
                CustomerEmail = invoice.CustomerEmail,
                CustomerAddress = invoice.CustomerAddress,
                Memo = invoice.Memo,
                Items = invoice.Items,
                Subtotal = invoice.Subtotal,
                Total = invoice.Total,
                Discount = invoice.Discount,
                TaxAmount = invoice.TaxAmount,
                TaxPercent = invoice.TaxPercent,
                Status = invoice.Status,
                TeamName = invoice.Team.Name,
                DueDate = DateTime.UtcNow.AddDays(14), //TODO: Set this value
            };
            if (invoice.Status == Invoice.StatusCodes.Paid || invoice.Status == Invoice.StatusCodes.Completed)
            {
                // add payment info
                model.PaidDate = invoice.Payment.OccuredAt;
            }

            return model;
        }

        [HttpGet]
        [MiddlewareFilter(typeof(JsReportPipeline))]
        public async Task<ActionResult> Pdf(string id)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Payment)
                .Include(i => i.Team)
                .FirstOrDefaultAsync(i => i.LinkId == id);

            if (invoice == null)
            {
                return NotFound();
            }

            var model = CreateInvoicePaymentViewModel(invoice);

            // TODO: Change this to ChromePdf when it's available on Local            
            HttpContext.JsReportFeature()
                .Recipe(Recipe.PhantomPdf);

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> Preview(int id)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Payment)
                .Include(i => i.Team)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            var model = new InvoicePaymentViewModel()
            {
                Id              = invoice.Id.ToString(),
                CustomerName    = invoice.CustomerName,
                CustomerEmail   = invoice.CustomerEmail,
                CustomerAddress = invoice.CustomerAddress,
                Memo            = invoice.Memo,
                Items           = invoice.Items,
                Subtotal        = invoice.Subtotal,
                Total           = invoice.Total,
                Discount        = invoice.Discount,
                TaxAmount       = invoice.TaxAmount,
                TaxPercent      = invoice.TaxPercent,
                Status          = invoice.Status,
                TeamName        = invoice.Team.Name,
            };

            return View(model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public ActionResult Receipt(ReceiptResponseModel response)
        {
            Log.ForContext("response", response, true).Information("Receipt response received");

            // check signature
            var dictionary = Request.Form.ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());
            if (!_dataSigningService.Check(dictionary, response.Signature))
            {
                Log.Error("Check Signature Failure");
                ViewBag.ErrorMessage = "An error has occurred. Payment not processed. If you experience further problems, contact us.";
                return RedirectToAction("Index", "Home");
            }
            ViewBag.PaymentDictionary = dictionary;

            // find matching invoice
            var invoice = _dbContext.Invoices.SingleOrDefault(a => a.Id == response.Req_Reference_Number);
            if (invoice == null)
            {
                Log.Error("Order not found {0}", response.Req_Reference_Number);
                ViewBag.ErrorMessage = "Invoice for payment not found. Please contact technical support.";
                return NotFound(response.Req_Reference_Number);
            }

            var model = new InvoicePaymentViewModel()
            {
                CustomerName    = invoice.CustomerName,
                CustomerEmail   = invoice.CustomerEmail,
                CustomerAddress = invoice.CustomerAddress,
                Memo            = invoice.Memo,
                Items           = invoice.Items,
                Subtotal        = invoice.Subtotal,
                Total           = invoice.Total,
                Discount        = invoice.Discount,
                TaxAmount       = invoice.TaxAmount,
                TaxPercent      = invoice.TaxPercent,
            };

            var responseValid = CheckResponse(response);
            if (!responseValid.IsValid)
            {
                // send them back to the pay page with errors
                ViewBag.ErrorMessage = string.Format("Errors detected: {0}", string.Join(",", responseValid.Errors));
                return View("Pay", model);
            }

            // Should be good,   
            ViewBag.Message = "Payment Processed. Thank You.";
            model.PaidDate = response.AuthorizationDateTime;
            model.Status = Invoice.StatusCodes.Paid;

            #region DEBUG
            // For testing local only, we should process the actual payment
            // Live systems will use the side channel message from cybersource direct to record the payment event
            var payment = ProcessPaymentEvent(response, dictionary);
            if (response.Decision == ReplyCodes.Accept)
            {
                invoice.Payment = payment;
                invoice.Status = Invoice.StatusCodes.Paid;
            }
            _dbContext.SaveChanges();
            #endregion

            return View("Pay", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public ActionResult ProviderNotify(ReceiptResponseModel response)
        {
            Log.ForContext("response", response, true).Information("Provider Notification Received");

            // check signature
            var dictionary = Request.Form.ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());
            if (!_dataSigningService.Check(dictionary, response.Signature))
            {
                Log.Error("Check Signature Failure");
                return new JsonResult(new { });
            }

            var payment = ProcessPaymentEvent(response, dictionary);

            var invoice = _dbContext.Invoices.SingleOrDefault(a => a.Id == response.Req_Reference_Number);
            if (invoice == null)
            {
                Log.Error("Invoice not found {0}", response.Req_Reference_Number);
                return new JsonResult(new { });
            }

            if (response.Decision == ReplyCodes.Accept)
            {
                invoice.Payment = payment;
                invoice.Status = "paid";
            }

            _dbContext.SaveChanges();
            return new JsonResult(new { });
        }

        private PaymentEvent ProcessPaymentEvent(ReceiptResponseModel response, Dictionary<string, string> dictionary)
        {
            var paymentEvent = new PaymentEvent
            {
                Transaction_Id       = response.Transaction_Id,
                Auth_Amount          = response.Auth_Amount,
                Decision             = response.Decision,
                Reason_Code          = response.Reason_Code,
                Req_Reference_Number = response.Req_Reference_Number,
                ReturnedResults      = JsonConvert.SerializeObject(dictionary)
            };

            _dbContext.PaymentEvents.Add(paymentEvent);

            return paymentEvent;
        }

        private CheckResponseResults CheckResponse(ReceiptResponseModel response)
        {
            var rtValue = new CheckResponseResults();
            //Ok, check response
            // general error, bad request
            if (string.Equals(response.Decision, ReplyCodes.Error) ||
                response.Reason_Code == ReasonCodes.BadRequestError ||
                response.Reason_Code == ReasonCodes.MerchantAccountError)
            {
                Log.ForContext("decision", response.Decision).ForContext("reason", response.Reason_Code)
                    .Warning("Unsuccessful Reply");
                rtValue.Errors.Add("An error has occurred. If you experience further problems, please contact us");
            }

            // this is only possible on a hosted payment page
            if (string.Equals(response.Decision, ReplyCodes.Cancel))
            {
                Log.ForContext("decision", response.Decision).ForContext("reason", response.Reason_Code).Warning("Cancelled Reply");
                rtValue.Errors.Add("The payment process was canceled before it could complete. If you experience further problems, please contact us");
            }

            // manual review required
            if (string.Equals(response.Decision, ReplyCodes.Review))
            {
                Log.ForContext("decision", response.Decision).ForContext("reason", response.Reason_Code).Warning("Manual Review Reply");
                rtValue.Errors.Add("Error with Credit Card. Please contact issuing bank. If you experience further problems, please contact us");
            }

            // bad cc information, return to payment page
            if (string.Equals(response.Decision, ReplyCodes.Decline))
            {
                if (response.Reason_Code == ReasonCodes.AvsFailure)
                {
                    Log.ForContext("decision", response.Decision).ForContext("reason", response.Reason_Code).Warning("Avs Failure");
                    rtValue.Errors.Add("We’re sorry, but it appears that the billing address that you entered does not match the billing address registered with your card. Please verify that the billing address and zip code you entered are the ones registered with your card issuer and try again. If you experience further problems, please contact us");
                }

                if (response.Reason_Code == ReasonCodes.BankTimeoutError ||
                    response.Reason_Code == ReasonCodes.ProcessorTimeoutError)
                {
                    Log.ForContext("decision", response.Decision).ForContext("reason", response.Reason_Code).Error("Bank Timeout Error");
                    rtValue.Errors.Add("Error contacting Credit Card issuing bank. Please wait a few minutes and try again. If you experience further problems, please contact us");
                }
                else
                {
                    Log.ForContext("decision", response.Decision).ForContext("reason", response.Reason_Code).Warning("Declined Card Error");
                    rtValue.Errors.Add("We’re sorry but your credit card was declined. Please use an alternative credit card and try submitting again. If you experience further problems, please contact us");
                }
            }

            // good cc info, partial payment
            if (string.Equals(response.Decision, ReplyCodes.Accept) &&
                response.Reason_Code == ReasonCodes.PartialApproveError)
            {
                //I Don't think this can happen.
                //TODO: credit card was partially billed. flag transaction for review
                //TODO: send to general error page
                Log.ForContext("decision", response.Decision).ForContext("reason", response.Reason_Code).Error("Partial Payment Error");
                rtValue.Errors.Add("We’re sorry but a Partial Payment Error was detected. Please contact us");
            }

            if (rtValue.Errors.Count <= 0)
            {
                if (response.Decision != ReplyCodes.Accept)
                {
                    Log.Error("Got past all the other checks. But it still wasn't Accepted");
                    rtValue.Errors.Add("Unknown Error. Please contact us.");
                }
                else
                {
                    rtValue.IsValid = true;
                }
            }

            return rtValue;
        }

        private class CheckResponseResults
        {
            public bool IsValid { get; set; } = false;
            public IList<string> Errors { get; set; } = new List<string>();
        }
    }
}
