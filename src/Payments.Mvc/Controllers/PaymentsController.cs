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
using Payments.Core.Models.History;
using Payments.Core.Models.Notifications;
using Payments.Core.Services;
using Payments.Mvc.Models.Configuration;
using Payments.Mvc.Models.CyberSource;
using Payments.Mvc.Models.PaymentViewModels;
using Payments.Mvc.Services;
using Serilog;

namespace Payments.Mvc.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDataSigningService _dataSigningService;
        private readonly INotificationService _notificationService;
        private readonly CyberSourceSettings _cyberSourceSettings;

        public PaymentsController(ApplicationDbContext dbContext, IDataSigningService dataSigningService, INotificationService notificationService, IOptions<CyberSourceSettings> cyberSourceSettings)
        {
            _dbContext = dbContext;
            _dataSigningService = dataSigningService;
            _notificationService = notificationService;
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

        [HttpGet]
        [MiddlewareFilter(typeof(JsReportPipeline))]
        public async Task<ActionResult> Pdf(string id)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Payment)
                .Include(i => i.Team)
                .FirstOrDefaultAsync(i => i.LinkId == id);

            if (invoice == null || string.IsNullOrWhiteSpace(id))
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

            var model = new PreviewInvoiceViewModel()
            {
                Id               = invoice.Id.ToString(),
                CustomerName     = invoice.CustomerName,
                CustomerEmail    = invoice.CustomerEmail,
                CustomerAddress  = invoice.CustomerAddress,
                DueDate          = invoice.DueDate,
                Memo             = invoice.Memo,
                Items            = invoice.Items,
                Subtotal         = invoice.Subtotal,
                Total            = invoice.Total,
                Discount         = invoice.Discount,
                TaxAmount        = invoice.TaxAmount,
                TaxPercent       = invoice.TaxPercent,
                TeamName         = invoice.Team.Name,
                TeamContactEmail = invoice.Team.ContactEmail,
                TeamContactPhone = invoice.Team.ContactPhoneNumber,
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult PreviewFromJson([FromForm(Name = "json")] string json)
        {
            var model = JsonConvert.DeserializeObject<PreviewInvoiceViewModel>(json);

            // fill in totals
            foreach (var i in model.Items)
            {
                i.Total = i.Amount * i.Quantity;
            }
            model.UpdateCalculatedValues();

            return View("preview", model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> Receipt(ReceiptResponseModel response)
        {
            Log.ForContext("response", response, true).Information("Receipt response received");

            // check signature
            var dictionary = Request.Form.ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());
            if (!_dataSigningService.Check(dictionary, response.Signature))
            {
                Log.Error("Check Signature Failure");
                ErrorMessage = "An error has occurred. Payment not processed. If you experience further problems, contact us.";
                return StatusCode(500);
            }

            // find matching invoice
            var invoice = await _dbContext.Invoices.SingleOrDefaultAsync(a => a.Id == response.Req_Reference_Number);
            if (invoice == null)
            {
                Log.Error("Order not found {0}", response.Req_Reference_Number);
                ErrorMessage = "Invoice for payment not found. Please contact technical support.";
                return NotFound();
            }

            var model = new PaymentInvoiceViewModel()
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
#if DEBUG
                // For testing local only, we should process the actual payment
                // record action
                var failureAction = new History()
                {
                    Type = HistoryActionTypes.PaymentFailed.TypeCode,
                    ActionDateTime = DateTime.UtcNow,
                };
                invoice.History.Add(failureAction);

                await _dbContext.SaveChangesAsync();
#endif

                // send them back to the pay page with errors
                ErrorMessage = string.Format("Errors detected: {0}", string.Join(",", responseValid.Errors));
                return View("Pay", model);
            }

            // Should be good,   
            Message = "Payment Processed. Thank You.";
            model.PaidDate = response.AuthorizationDateTime;
            model.Status = Invoice.StatusCodes.Paid;

#if DEBUG
            // For testing local only, we should process the actual payment
            // Live systems will use the side channel message from cybersource direct to record the payment event
            ViewBag.PaymentDictionary = dictionary;
            var payment = ProcessPaymentEvent(response, dictionary);
            if (response.Decision == ReplyCodes.Accept)
            {
                invoice.Payment = payment;
                invoice.Status = Invoice.StatusCodes.Paid;
            }

            // record action
            var successAction = new History()
            {
                Type = HistoryActionTypes.PaymentCompleted.TypeCode,
                ActionDateTime = DateTime.UtcNow,
            };
            invoice.History.Add(successAction);

            await _dbContext.SaveChangesAsync();
#endif

            return View("Pay", model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> Cancel(ReceiptResponseModel response)
        {
            Log.ForContext("response", response, true).Information("Receipt response received");

            // check signature
            var dictionary = Request.Form.ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());
            if (!_dataSigningService.Check(dictionary, response.Signature))
            {
                Log.Error("Check Signature Failure");
                ErrorMessage = "An error has occurred. Payment not processed. If you experience further problems, contact us.";
                return StatusCode(500);
            }

            // find matching invoice
            var invoice = _dbContext.Invoices.SingleOrDefault(a => a.Id == response.Req_Reference_Number);
            if (invoice == null)
            {
                Log.Error("Order not found {0}", response.Req_Reference_Number);
                ErrorMessage = "Invoice for payment not found. Please contact technical support.";
                return NotFound();
            }

#if DEBUG
            // record action
            var action = new History()
            {
                Type = HistoryActionTypes.PaymentFailed.TypeCode,
                ActionDateTime = DateTime.UtcNow,
            };
            invoice.History.Add(action);
            await _dbContext.SaveChangesAsync();
#endif

            ErrorMessage = "Payment Process Cancelled";
            return RedirectToAction(nameof(Pay), new {id = invoice.LinkId});
        }

        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> ProviderNotify(ReceiptResponseModel response)
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
                invoice.Status = Invoice.StatusCodes.Paid;

                // record action
                var action = new History()
                {
                    Type = HistoryActionTypes.PaymentCompleted.TypeCode,
                    ActionDateTime = DateTime.UtcNow,
                };
                invoice.History.Add(action);

                // process notifications
                await _notificationService.SendPaidNotification(new PaidNotification()
                {
                    InvoiceId = invoice.Id
                });
            }
            else
            {
                // record action
                var action = new History()
                {
                    Type = HistoryActionTypes.PaymentFailed.TypeCode,
                    ActionDateTime = DateTime.UtcNow,
                };
                invoice.History.Add(action);
            }

            await _dbContext.SaveChangesAsync();
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
        private PaymentInvoiceViewModel CreateInvoicePaymentViewModel(Invoice invoice)
        {
            // update team contact info
            var model = new PaymentInvoiceViewModel()
            {
                Id               = invoice.Id.ToString(),
                LinkId           = invoice.LinkId,
                CustomerName     = invoice.CustomerName,
                CustomerEmail    = invoice.CustomerEmail,
                CustomerAddress  = invoice.CustomerAddress,
                Memo             = invoice.Memo,
                Items            = invoice.Items,
                Subtotal         = invoice.Subtotal,
                Total            = invoice.Total,
                Discount         = invoice.Discount,
                TaxAmount        = invoice.TaxAmount,
                TaxPercent       = invoice.TaxPercent,
                Status           = invoice.Status,
                DueDate          = invoice.DueDate,
                TeamName         = invoice.Team.Name,
                TeamContactEmail = invoice.Team.ContactEmail,
                TeamContactPhone = invoice.Team.ContactPhoneNumber,
            };

            // add payment info
            if (invoice.Status == Invoice.StatusCodes.Paid || invoice.Status == Invoice.StatusCodes.Completed)
            {
                model.PaidDate = invoice.Payment.OccuredAt;
            }

            return model;
        }

        private const string TempDataMessageKey = "Message";
        private string Message
        {
            get => TempData[TempDataMessageKey] as string;
            set => TempData[TempDataMessageKey] = value;
        }

        private const string TempDataErrorMessageKey = "ErrorMessage";
        private string ErrorMessage
        {
            get => TempData[TempDataErrorMessageKey] as string;
            set => TempData[TempDataErrorMessageKey] = value;
        }
    }
}
