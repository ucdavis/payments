using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Extensions;
using Payments.Core.Models.Configuration;
using Payments.Core.Models.History;
using Payments.Core.Models.Notifications;
using Payments.Core.Resources;
using Payments.Core.Services;
using Payments.Emails;
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
        private readonly IStorageService _storageService;
        private readonly IEmailService _emailService;

        private readonly CyberSourceSettings _cyberSourceSettings;

        public PaymentsController(ApplicationDbContext dbContext, IDataSigningService dataSigningService, INotificationService notificationService, IStorageService storageService, IOptions<CyberSourceSettings> cyberSourceSettings, IEmailService emailService)
        {
            _dbContext = dbContext;
            _dataSigningService = dataSigningService;
            _notificationService = notificationService;
            _storageService = storageService;
            _emailService = emailService;

            _cyberSourceSettings = cyberSourceSettings.Value;
        }

        [HttpGet]
        public async Task<ActionResult> Pay(string id)
        {
            //Changes here should be made to Download too
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Team)
                .Include(i => i.Attachments)
                .Include(i => i.Coupon)
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
                    return RedirectToAction("Pay", new {id = link.Invoice.LinkId});
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
        public async Task<ActionResult> Download(string id)
        {
            //This is a copy of Pay, changes there should be reflected here
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Team)
                .Include(i => i.Attachments)
                .Include(i => i.Coupon)
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
                    return RedirectToAction("Download", new { id = link.Invoice.LinkId });
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

            var model = CreateInvoicePaymentViewModel(invoice);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> AddCoupon(string id, string code)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Team)
                .Include(i => i.Attachments)
                .Include(i => i.Coupon)
                .FirstOrDefaultAsync(i => i.LinkId == id);

            if (invoice == null)
            {
                return PublicNotFound();
            }

            // the customer isn't allowed access to draft or cancelled invoices
            if (invoice.Status == Invoice.StatusCodes.Draft || invoice.Status == Invoice.StatusCodes.Cancelled)
            {
                return PublicNotFound();
            }

            // the customer isn't allowed to modify a paid invoice
            if (invoice.Paid)
            {
                ErrorMessage = "The invoice has already been paid.";
                return RedirectToAction("Pay", new {id});
            }

            // the customer isn't allowed to add another coupon
            if (invoice.Coupon != null)
            {
                ErrorMessage = "The invoice already has a coupon. To add a different coupon, please remove the current one.";
                return RedirectToAction("Pay", new { id });
            }

            // find the coupon on same team
            var coupon = await _dbContext.Coupons
                .Where(c => c.Team.Slug == invoice.Team.Slug)
                .FirstOrDefaultAsync(c => string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase));

            if (coupon == null)
            {
                ErrorMessage = "Coupon code not found.";
                return RedirectToAction("Pay", new { id });
            }

            if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt.Value < DateTime.UtcNow.ToPacificTime().Date)
            {
                ErrorMessage = "Coupon code has expired.";
                return RedirectToAction("Pay", new { id });
            }

            // add the coupon
            invoice.Coupon = coupon;
            Message = "Coupon added.";

            // record event
            var action = new History()
            {
                Type = HistoryActionTypes.CouponAddedByCustomer.TypeCode,
                ActionDateTime = DateTime.UtcNow,
            };
            invoice.History.Add(action);

            // update totals
            invoice.UpdateCalculatedValues();
            await _dbContext.SaveChangesAsync();

            // return to pay page
            return RedirectToAction("Pay", new { id });
        }

        [HttpPost]
        public async Task<ActionResult> RemoveCoupon(string id)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Team)
                .Include(i => i.Attachments)
                .Include(i => i.Coupon)
                .FirstOrDefaultAsync(i => i.LinkId == id);

            if (invoice == null)
            {
                return PublicNotFound();
            }

            // the customer isn't allowed access to draft or cancelled invoices
            if (invoice.Status == Invoice.StatusCodes.Draft || invoice.Status == Invoice.StatusCodes.Cancelled)
            {
                return PublicNotFound();
            }

            // the customer isn't allowed to modify a paid invoice
            if (invoice.Paid)
            {
                ErrorMessage = "The invoice has already been paid.";
                return RedirectToAction("Pay", new { id });
            }

            // the customer isn't allowed to remove no coupon
            if (invoice.Coupon == null)
            {
                ErrorMessage = "The invoice doesn't have a coupon.";
                return RedirectToAction("Pay", new { id });
            }

            // remove the coupon and any old discount
            invoice.Coupon = null;
            invoice.ManualDiscount = 0;
            Message = "Coupon removed.";

            // record event
            var action = new History()
            {
                Type = HistoryActionTypes.CouponRemovedByCustomer.TypeCode,
                ActionDateTime = DateTime.UtcNow,
            };
            invoice.History.Add(action);

            // update totals
            invoice.UpdateCalculatedValues();
            await _dbContext.SaveChangesAsync();

            // return to pay page
            return RedirectToAction("Pay", new { id });
        }

        [HttpGet]
        public async Task<ActionResult> File(string id, int fileId)
        {
            if (string.IsNullOrWhiteSpace(id) || fileId <= 0)
            {
                return PublicNotFound();
            }

            // fetch invoice and attachments
            var invoice = await _dbContext.Invoices
                .Include(i => i.Attachments)
                .FirstOrDefaultAsync(i => i.LinkId == id);
            if (invoice == null)
            {
                return PublicNotFound();
            }

            var attachment = invoice.Attachments.FirstOrDefault(a => a.Id == fileId);
            if (attachment == null)
            {
                return PublicNotFound();
            }

            // get file
            var blob = await _storageService.DownloadFile(attachment.Identifier, StorageSettings.AttachmentContainerName);
            var stream = await blob.OpenReadAsync();

            // ship it
            return File(stream, attachment.ContentType, attachment.FileName);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> Preview(int id)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Team)
                .Include(i => i.Attachments)
                .Include(i => i.Coupon)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            invoice.UpdateCalculatedValues();

            var model = new PreviewInvoiceViewModel()
            {
                Id               = invoice.GetFormattedId(),
                CustomerName     = invoice.CustomerName,
                CustomerCompany  = invoice.CustomerCompany,
                CustomerEmail    = invoice.CustomerEmail,
                CustomerAddress  = invoice.CustomerAddress,
                DueDate          = invoice.DueDate,
                Memo             = invoice.Memo,
                Items            = invoice.Items,
                Attachments      = invoice.Attachments,
                Coupon           = invoice.Coupon,
                Discount         = invoice.CalculatedDiscount,
                TaxAmount        = invoice.CalculatedTaxAmount,
                TaxPercent       = invoice.TaxPercent,
                Subtotal         = invoice.CalculatedSubtotal,
                Total            = invoice.CalculatedTotal,
                Paid             = invoice.Paid,
                PaidDate         = invoice.PaidAt.ToPacificTime(),
                Team             = new PaymentInvoiceTeamViewModel(invoice.Team),
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult PreviewFromJson([FromForm(Name = "json")] string json)
        {
            var model = JsonConvert.DeserializeObject<PreviewInvoiceViewModel>(json);

            return View("preview", model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> Receipt(ReceiptResponseModel response)
        {
#if DEBUG
            // For testing local only, we should process the actual payment
            // Live systems will use the side channel message from cybersource direct to record the payment event
            // This will duplicate some log messages. That is OKAY
            await ProviderNotify(response);
#endif

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
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Team)
                .Include(i => i.Attachments)
                .Include(i => i.Coupon)
                .SingleOrDefaultAsync(a => a.Id == response.Req_Reference_Number);

            if (invoice == null)
            {
                Log.Error("Order not found {0}", response.Req_Reference_Number);
                ErrorMessage = "Invoice for payment not found. Please contact technical support.";
                return PublicNotFound();
            }

            var responseValid = CheckResponse(response);
            if (!responseValid.IsValid)
            {
                // send them back to the pay page with errors
                ErrorMessage = string.Format("Errors detected: {0}", string.Join(",", responseValid.Errors));
                return RedirectToAction(nameof(Pay), new { id = invoice.LinkId });
            }

            // Should be good 
            Message = "Payment Processed. Thank You.";

            // Fake payment status
            var model = CreateInvoicePaymentViewModel(invoice);
            model.Paid = true;
            model.PaidDate = response.AuthorizationDateTime;

            return View("Pay", model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> Cancel(ReceiptResponseModel response)
        {
#if DEBUG
            // For testing local only, we should process the actual payment
            // Live systems will use the side channel message from cybersource direct to record the payment event
            // This will duplicate some log messages. That is OKAY
            await ProviderNotify(response);
#endif

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
                return PublicNotFound();
            }

            ErrorMessage = "Payment Process Cancelled";
            return RedirectToAction(nameof(Pay), new { id = invoice.LinkId });
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

            // record payment process in db
            var payment = await ProcessPaymentEvent(response, dictionary);

            // try to find matching invoice
            var invoice = _dbContext.Invoices
                .Include(i => i.Items)
                .Include(i => i.Team)
                .Include(i => i.Coupon)
                .SingleOrDefault(a => a.Id == response.Req_Reference_Number);

            if (invoice == null)
            {
                Log.Error("Invoice not found {0}", response.Req_Reference_Number);
                return new JsonResult(new { });
            }

            // associate invoice
            payment.Invoice = invoice;

            if (response.Decision == ReplyCodes.Accept)
            {
                invoice.ManualDiscount = invoice.ManualDiscount >= 0 ? invoice.ManualDiscount : invoice.GetDiscountAmount(); //Before it is set to paid. Even this way, depending when it happens, the notify might happen after the coupon expires.
                invoice.Status = Invoice.StatusCodes.Paid;
                invoice.Paid = true;
                invoice.PaidAt = response.AuthorizationDateTime;
                invoice.PaymentType = PaymentTypes.CreditCard;
                invoice.PaymentProcessorId = response.Transaction_Id;
                
                invoice.UpdateCalculatedValues(); //Need to do after it is paid because they may not have got an expired discount?

                // record action
                var action = new History()
                {
                    Type = HistoryActionTypes.PaymentCompleted.TypeCode,
                    ActionDateTime = DateTime.UtcNow,
                };
                invoice.History.Add(action);

                // send email
                try
                {
                    await _emailService.SendReceipt(invoice, payment);
                }
                catch (Exception err)
                {
                    Log.Error(err, "Error while trying to send receipt.");
                }

                // process notifications
                try
                {
                    await _notificationService.SendPaidNotification(new PaidNotification()
                    {
                        InvoiceId = invoice.Id
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while sending notification.");
                }
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

        private async Task<PaymentEvent> ProcessPaymentEvent(ReceiptResponseModel response, Dictionary<string, string> dictionary)
        {
            // create and record event
            var paymentEvent = new PaymentEvent
            {
                Processor         = "CyberSource",
                ProcessorId       = response.Transaction_Id,
                Decision          = response.Decision,
                OccuredAt         = response.AuthorizationDateTime,
                BillingFirstName  = response.Req_Bill_To_Forename,
                BillingLastName   = response.Req_Bill_To_Surname,
                BillingEmail      = response.Req_Bill_To_Email,
                BillingCompany    = response.Req_Bill_To_Company_Name,
                BillingPhone      = response.Req_Bill_To_Phone,
                BillingStreet1    = response.Req_Bill_To_Address_Line1,
                BillingStreet2    = response.Req_Bill_To_Address_Line2,
                BillingCity       = response.Req_Bill_To_Address_City,
                BillingState      = response.Req_Bill_To_Address_State,
                BillingCountry    = response.Req_Bill_To_Address_Country,
                BillingPostalCode = response.Req_Bill_To_Address_Postal_Code,
                CardType          = response.Req_Card_Type,
                CardNumber        = response.Req_Card_Number,
                CardExpiry        = response.CardExpiration,
                ReturnedResults   = JsonConvert.SerializeObject(dictionary),
            };

            if (decimal.TryParse(response.Auth_Amount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture , out decimal amount))
            {
                paymentEvent.Amount = amount;
            }

            _dbContext.PaymentEvents.Add(paymentEvent);
            await _dbContext.SaveChangesAsync();

            return paymentEvent;
        }

        private CheckResponseResults CheckResponse(ReceiptResponseModel response)
        {
            var contextLog = Log.ForContext("decision", response.Decision).ForContext("reason", response.Reason_Code);

            var rtValue = new CheckResponseResults();
            //Ok, check response
            // general error, bad request
            if (string.Equals(response.Decision, ReplyCodes.Error) ||
                response.Reason_Code == ReasonCodes.BadRequestError ||
                response.Reason_Code == ReasonCodes.MerchantAccountError)
            {
                contextLog.Warning("Unsuccessful Reply");
                rtValue.Errors.Add("An error has occurred. If you experience further problems, please contact us");
            }

            // this is only possible on a hosted payment page
            if (string.Equals(response.Decision, ReplyCodes.Cancel))
            {
                contextLog.Warning("Cancelled Reply");
                rtValue.Errors.Add("The payment process was canceled before it could complete. If you experience further problems, please contact us");
            }

            // manual review required
            if (string.Equals(response.Decision, ReplyCodes.Review))
            {
                contextLog.Warning("Manual Review Reply");
                rtValue.Errors.Add("Error with Credit Card. Please contact issuing bank. If you experience further problems, please contact us");
            }

            // bad cc information, return to payment page
            if (string.Equals(response.Decision, ReplyCodes.Decline))
            {
                if (response.Reason_Code == ReasonCodes.AvsFailure)
                {
                    contextLog.Warning("Avs Failure");
                    rtValue.Errors.Add("We’re sorry, but it appears that the billing address that you entered does not match the billing address registered with your card. Please verify that the billing address and zip code you entered are the ones registered with your card issuer and try again. If you experience further problems, please contact us");
                }

                if (response.Reason_Code == ReasonCodes.BankTimeoutError ||
                    response.Reason_Code == ReasonCodes.ProcessorTimeoutError)
                {
                    contextLog.Error("Bank Timeout Error");
                    rtValue.Errors.Add("Error contacting Credit Card issuing bank. Please wait a few minutes and try again. If you experience further problems, please contact us");
                }
                else
                {
                    contextLog.Warning("Declined Card Error");
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
                contextLog.Error("Partial Payment Error");
                rtValue.Errors.Add("We’re sorry but a Partial Payment Error was detected. Please contact us");
            }

            if (rtValue.Errors.Count <= 0)
            {
                if (response.Decision != ReplyCodes.Accept)
                {
                    contextLog.Error("Got past all the other checks. But it still wasn't Accepted");
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
            var model = new PaymentInvoiceViewModel()
            {
                Id               = invoice.GetFormattedId(),
                LinkId           = invoice.LinkId,
                CustomerName     = invoice.CustomerName,
                CustomerCompany  = invoice.CustomerCompany,
                CustomerEmail    = invoice.CustomerEmail,
                CustomerAddress  = invoice.CustomerAddress,
                Memo             = invoice.Memo,
                Items            = invoice.Items,
                Attachments      = invoice.Attachments,
                Coupon           = invoice.Coupon,
                Discount         = invoice.CalculatedDiscount,
                TaxAmount        = invoice.CalculatedTaxAmount,
                TaxPercent       = invoice.TaxPercent,
                Subtotal         = invoice.CalculatedSubtotal,
                Total            = invoice.CalculatedTotal,
                DueDate          = invoice.DueDate,
                Paid             = invoice.Paid,
                PaidDate         = invoice.PaidAt.ToPacificTime(),
                Team             = new PaymentInvoiceTeamViewModel(invoice.Team),
            };

            return model;
        }

        /// <summary>
        /// override NotFound to return 
        /// </summary>
        /// <returns></returns>
        public ActionResult PublicNotFound()
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
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
