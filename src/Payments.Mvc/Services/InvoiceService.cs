using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Helpers;
using Payments.Core.Models.Invoice;
using Payments.Core.Services;
using Payments.Emails;

namespace Payments.Mvc.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailService _emailService;

        public InvoiceService(ApplicationDbContext dbContext, IEmailService emailService)
        {
            _dbContext = dbContext;
            _emailService = emailService;
        }

        public async Task<IReadOnlyList<Invoice>> CreateInvoices(CreateInvoiceModel model, Team team)
        {
            // find account
            var account = await _dbContext.FinancialAccounts
                .FirstOrDefaultAsync(a => a.Team.Id == team.Id && a.Id == model.AccountId);

            if (account == null)
            {
                throw new ArgumentException("Account Id not found for this team.", nameof(model.AccountId));
            }

            if (!account.IsActive)
            {
                throw new ArgumentException("Account is inactive.", nameof(model.AccountId));
            }

            // find coupon
            Coupon coupon = null;
            if (model.CouponId > 0)
            {
                coupon = await _dbContext.Coupons
                    .FirstOrDefaultAsync(c => c.Team.Id == team.Id && c.Id == model.CouponId);
            }

            // manage multiple customer scenario
            var invoices = new List<Invoice>();
            foreach (var customer in model.Customers)
            {
                // create new object, track it
                var invoice = new Invoice
                {
                    DraftCount      = 1,
                    Account         = account,
                    Coupon          = coupon,
                    Team            = team,
                    ManualDiscount  = model.ManualDiscount,
                    TaxPercent      = model.TaxPercent,
                    DueDate         = model.DueDate,
                    CustomerAddress = customer.Address,
                    CustomerEmail   = customer.Email,
                    CustomerName    = customer.Name,
                    CustomerCompany = customer.Company,
                    Memo            = model.Memo,
                    Status          = Invoice.StatusCodes.Draft,
                    Sent            = false,
                    Type            = model.Type,
                };

                // add line items
                var items = model.Items.Select(i => new LineItem()
                {
                    Amount      = i.Amount,
                    Description = i.Description,
                    Quantity    = i.Quantity,
                    TaxExempt   = i.TaxExempt,
                    Total       = i.Quantity * i.Amount,
                });
                invoice.Items = items.ToList();

                // add attachments
                var attachments = model.Attachments.Where(a => !string.IsNullOrWhiteSpace(a.Identifier)).Select(a => new InvoiceAttachment()
                {
                    Identifier  = a.Identifier,
                    FileName    = a.FileName,
                    ContentType = a.ContentType,
                    Size        = a.Size,
                });
                invoice.Attachments = attachments.ToList();

                // start tracking for db
                invoice.UpdateCalculatedValues();

                if (invoice.CalculatedTotal <= 0)
                {
                    throw new ArgumentException("Invoice total must be greater than 0.", nameof(model));
                }

                _dbContext.Invoices.Add(invoice);

                invoices.Add(invoice);
            }

            return invoices;
        }

        public async Task<Invoice> UpdateInvoice(Invoice invoice, EditInvoiceModel model)
        {
            // get team out of invoice
            var team = invoice.Team;

            // find account
            var account = await _dbContext.FinancialAccounts
                .FirstOrDefaultAsync(a => a.Team.Id == team.Id && a.Id == model.AccountId);

            if (account == null)
            {
                throw new ArgumentException("Account Id not found for this team.", nameof(model.AccountId));
            }

            if (!account.IsActive)
            {
                throw new ArgumentException("Account is inactive.", nameof(model.AccountId));
            }

            // find coupon
            Coupon coupon = null;
            if (model.CouponId > 0)
            {
                coupon = await _dbContext.Coupons
                    .FirstOrDefaultAsync(c => c.Team.Id == team.Id && c.Id == model.CouponId);
            }

            // TODO: Consider modifying items instead of replacing
            // remove old items
            _dbContext.LineItems.RemoveRange(invoice.Items);

            // update invoice
            invoice.Account         = account;
            invoice.Coupon          = coupon;
            invoice.CustomerAddress = model.Customer.Address;
            invoice.CustomerEmail   = model.Customer.Email;
            invoice.CustomerName    = model.Customer.Name;
            invoice.CustomerCompany = model.Customer.Company;
            invoice.Memo            = model.Memo;
            invoice.ManualDiscount  = model.ManualDiscount;
            invoice.TaxPercent      = model.TaxPercent;
            invoice.DueDate         = model.DueDate;

            // increase draft count
            invoice.DraftCount++;

            // add line items
            var items = model.Items.Select(i => new LineItem()
            {
                Amount      = i.Amount,
                Description = i.Description,
                Quantity    = i.Quantity,
                TaxExempt   = i.TaxExempt,
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

            invoice.UpdateCalculatedValues();

            if (invoice.CalculatedTotal <= 0)
            {
                throw new ArgumentException("Invoice total must be greater than 0.", nameof(model));
            }
            
            return invoice;
        }

        public async Task SendInvoice(Invoice invoice, SendInvoiceModel model)
        {
            // don't reset the key if it's already live
            if (!invoice.Sent)
            {
                SetInvoiceKey(invoice);
            }

            await _emailService.SendInvoice(invoice, model.ccEmails, model.bccEmails);

            invoice.Status = Invoice.StatusCodes.Sent;
            invoice.Sent = true;
            invoice.SentAt = DateTime.UtcNow;
        }

        public async Task SendReceipt(Invoice invoice, PaymentEvent payment, SendInvoiceModel model)
        {
            await _emailService.SendReceipt(invoice, payment);

            invoice.Status = Invoice.StatusCodes.Sent;
            invoice.Sent = true;
            invoice.SentAt = DateTime.UtcNow;
        }

        public async Task RefundInvoice(Invoice invoice, PaymentEvent payment, string refundReason, User user)
        {
            await _emailService.SendRefundRequest(invoice, payment, refundReason, user);

            invoice.Status = Invoice.StatusCodes.Refunding;
        }

        public string SetInvoiceKey(Invoice invoice)
        {
            // setup random 10 character key link id
            var linkId = InvoiceKeyHelper.GetUniqueKey();

            // append invoice id and draft
            linkId = $"{linkId}-{invoice.GetFormattedId()}";

            // create db row for tracking links
            var link = new InvoiceLink()
            {
                Invoice = invoice,
                LinkId = linkId,
                Expired = false,
            };
            _dbContext.InvoiceLinks.Add(link);

            // set key for fast recovery
            invoice.LinkId = linkId;

            return linkId;
        }
    }

    public interface IInvoiceService
    {
        Task<IReadOnlyList<Invoice>> CreateInvoices(CreateInvoiceModel model, Team team);

        Task<Invoice> UpdateInvoice(Invoice invoice, EditInvoiceModel model);

        Task SendInvoice(Invoice invoice, SendInvoiceModel model);

        Task RefundInvoice(Invoice invoice, PaymentEvent payment, string refundReason, User user);

        string SetInvoiceKey(Invoice invoice);
    }
}
