using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Helpers;
using Payments.Core.Models.History;
using Payments.Core.Models.Invoice;
using Payments.Core.Models.Validation;
using Payments.Core.Services;
using Payments.Emails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payments.Mvc.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly IAggieEnterpriseService _aggieEnterpriseService;

        public InvoiceService(ApplicationDbContext dbContext, IEmailService emailService, IAggieEnterpriseService aggieEnterpriseService)
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _aggieEnterpriseService = aggieEnterpriseService;
        }

        public async Task<IReadOnlyList<Invoice>> CreateInvoices(CreateInvoiceModel model, Team team)
        {
            //Account isn't really used or needed for recharge invoices, but keep doing this until we can see if it breaks anything.
            // find account
            var account = await _dbContext.FinancialAccounts
                .FirstOrDefaultAsync(a => a.Team.Id == team.Id && a.Id == model.AccountId);

            if (account == null && model.Type != Invoice.InvoiceTypes.Recharge)
            {
                throw new ArgumentException("Account Id not found for this team.", nameof(model.AccountId));
            }

            if (model.Type != Invoice.InvoiceTypes.Recharge && !account.IsActive)
            {
                throw new ArgumentException("Account is inactive.", nameof(model.AccountId));
            }


            if (model.Type == Invoice.InvoiceTypes.Recharge)
            {
                //Must have at least one credit recharge account.
                //All recharge accounts must be valid.
                //All credit recharge account must be 100% of total.
                //If any debit recharge accounts, they must all be 100% of total.

                if(model.RechargeAccounts == null || !model.RechargeAccounts.Any(a => a.Direction == RechargeAccount.CreditDebit.Credit))
                {
                    throw new ArgumentException("At least one credit recharge account is required for recharge invoices.", nameof(model.RechargeAccounts));
                }

                if(model.RechargeAccounts.Where(a => a.Amount <=0.0m ).Any())
                {
                    throw new ArgumentException("Recharge account amounts must be greater than 0.", nameof(model.RechargeAccounts));
                }

                foreach(var ra in model.RechargeAccounts)
                {
                    var validationModel = await _aggieEnterpriseService.IsRechargeAccountValid(ra.FinancialSegmentString, ra.Direction);
                    if(!validationModel.IsValid)
                    {
                        throw new ArgumentException($"Recharge account '{ra.FinancialSegmentString}' is not valid");
                    }
                    // Ok, this could be called from an API, so we need to replace and chart strings that may get changed.
                    if(validationModel.ChartString != ra.FinancialSegmentString)
                    {
                        ra.FinancialSegmentString = validationModel.ChartString;
                        //log it?
                    }
                }

                if(model.CouponId > 0)
                {
                    throw new ArgumentException("Coupons are not allowed for recharge invoices.", nameof(model.CouponId));
                }

                if(model.TaxPercent > 0.0m)
                {
                    throw new ArgumentException("Tax is not allowed for recharge invoices.", nameof(model.TaxPercent));
                }
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
                    DraftCount = 1,
                    Account = model.Type != Invoice.InvoiceTypes.Recharge ? account : null,
                    Coupon = coupon,
                    Team = team,
                    ManualDiscount = model.ManualDiscount,
                    TaxPercent = model.TaxPercent,
                    DueDate = model.DueDate,
                    CustomerAddress = customer.Address,
                    CustomerEmail = customer.Email,
                    CustomerName = customer.Name,
                    CustomerCompany = customer.Company,
                    Memo = model.Memo,
                    Status = Invoice.StatusCodes.Draft,
                    Sent = false,
                    Type = model.Type,
                };

                // add line items
                var items = model.Items.Select(i => new LineItem()
                {
                    Amount = i.Amount,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    TaxExempt = i.TaxExempt,
                    Total = i.Quantity * i.Amount,
                });
                invoice.Items = items.ToList();

                // add attachments
                var attachments = model.Attachments.Where(a => !string.IsNullOrWhiteSpace(a.Identifier)).Select(a => new InvoiceAttachment()
                {
                    Identifier = a.Identifier,
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    Size = a.Size,
                });
                invoice.Attachments = attachments.ToList();

                // start tracking for db
                invoice.UpdateCalculatedValues();

                if (invoice.CalculatedTotal <= 0)
                {
                    throw new ArgumentException("Invoice total must be greater than 0.", nameof(model));
                }

                if (model.Type == Invoice.InvoiceTypes.Recharge)
                {
                    var rechargeAccounts = model.RechargeAccounts.Select(a => new RechargeAccount()
                    {
                        Direction = a.Direction,
                        FinancialSegmentString = a.FinancialSegmentString,
                        Amount = a.Amount,
                        Percentage = a.Percentage,
                        EnteredByKerb = a.EnteredByKerb,
                        EnteredByName = a.EnteredByName,
                        Notes = a.Notes,

                    });
                    invoice.RechargeAccounts = rechargeAccounts.ToList();

                    if(invoice.CalculatedTotal != invoice.RechargeAccounts.Where(a => a.Direction == RechargeAccount.CreditDebit.Credit).Sum(a => a.Amount))
                    {
                        throw new ArgumentException("Total of credit recharge accounts must equal invoice total.", nameof(model.RechargeAccounts));
                    }
                    if(invoice.RechargeAccounts.Where(a => a.Direction == RechargeAccount.CreditDebit.Debit).Any() &&
                        invoice.CalculatedTotal != invoice.RechargeAccounts.Where(a => a.Direction == RechargeAccount.CreditDebit.Debit).Sum(a => a.Amount))
                    {
                        throw new ArgumentException("Total of debit recharge accounts must equal invoice total when supplied.", nameof(model.RechargeAccounts));
                    }
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

            //TODO: Continue to do the account for now, but we may not need it for recharge invoices.
            // find account
            var account = await _dbContext.FinancialAccounts
                .FirstOrDefaultAsync(a => a.Team.Id == team.Id && a.Id == model.AccountId);

            if (account == null && invoice.Type != Invoice.InvoiceTypes.Recharge)
            {
                throw new ArgumentException("Account Id not found for this team.", nameof(model.AccountId));
            }

            if (invoice.Type != Invoice.InvoiceTypes.Recharge && !account.IsActive)
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
            invoice.Account = invoice.Type != Invoice.InvoiceTypes.Recharge ?  account : null;
            invoice.Coupon =  coupon;
            invoice.CustomerAddress = model.Customer.Address;
            invoice.CustomerEmail = model.Customer.Email;
            invoice.CustomerName = model.Customer.Name;
            invoice.CustomerCompany = model.Customer.Company;
            invoice.Memo = model.Memo;
            invoice.ManualDiscount = model.ManualDiscount;
            invoice.TaxPercent = model.TaxPercent;
            invoice.DueDate = model.DueDate;

            // increase draft count
            invoice.DraftCount++;

            // add line items
            var items = model.Items.Select(i => new LineItem()
            {
                Amount = i.Amount,
                Description = i.Description,
                Quantity = i.Quantity,
                TaxExempt = i.TaxExempt,
                Total = i.Quantity * i.Amount,
            });
            invoice.Items = items.ToList();

            //TODO: Move this into a private method. Need to do on update too.
            if (invoice.Type == Invoice.InvoiceTypes.Recharge)
            {
                // Recharges don't have tax
                model.TaxPercent = 0;
                invoice.TaxPercent = 0;
                if(model.CouponId > 0)
                {
                    throw new ArgumentException("Coupons are not allowed for recharge invoices.", nameof(model.CouponId));
                }

                // remove old recharge accounts
                // If we do it this way, there may be issues if an invoice is rejected, then we edit it and resend. Now all the entered by values are lost.
                _dbContext.RechargeAccounts.RemoveRange(invoice.RechargeAccounts);

                var rechargeAccounts = model.RechargeAccounts.Select(a => new RechargeAccount()
                {
                    Direction = a.Direction,
                    FinancialSegmentString = a.FinancialSegmentString,
                    Amount = a.Amount,
                    Percentage = a.Percentage,
                    EnteredByKerb = a.EnteredByKerb,
                    EnteredByName = a.EnteredByName,
                    Notes = a.Notes,

                });
                invoice.RechargeAccounts = rechargeAccounts.ToList();

                // Validate recharge accounts again.
                if (invoice.RechargeAccounts == null || !invoice.RechargeAccounts.Any(a => a.Direction == RechargeAccount.CreditDebit.Credit))
                {
                    throw new ArgumentException("At least one credit recharge account is required for recharge invoices.", nameof(model.RechargeAccounts));
                }

                if (invoice.RechargeAccounts.Where(a => a.Amount <= 0.0m).Any())
                {
                    throw new ArgumentException("Recharge account amounts must be greater than 0.", nameof(model.RechargeAccounts));
                }

                foreach (var ra in invoice.RechargeAccounts)
                {
                    var validationModel = await _aggieEnterpriseService.IsRechargeAccountValid(ra.FinancialSegmentString, ra.Direction);
                    if (!validationModel.IsValid)
                    {
                        throw new ArgumentException($"Recharge account '{ra.FinancialSegmentString}' is not valid");
                    }
                    // Ok, this could be called from an API, so we need to replace and chart strings that may get changed.
                    if (validationModel.ChartString != ra.FinancialSegmentString)
                    {
                        ra.FinancialSegmentString = validationModel.ChartString;
                        //log it?
                    }
                }
            }

            // add attachments
            var attachments = model.Attachments.Select(a => new InvoiceAttachment()
            {
                Identifier = a.Identifier,
                FileName = a.FileName,
                ContentType = a.ContentType,
                Size = a.Size,
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

            if (invoice.Type == Invoice.InvoiceTypes.Recharge)
            {

                if (invoice.CalculatedTotal != invoice.RechargeAccounts.Where(a => a.Direction == RechargeAccount.CreditDebit.Credit).Sum(a => a.Amount))
                {
                    throw new ArgumentException("Total of credit recharge accounts must equal invoice total.", nameof(model.RechargeAccounts));
                }
                if (invoice.RechargeAccounts.Where(a => a.Direction == RechargeAccount.CreditDebit.Debit).Any() &&
                    invoice.CalculatedTotal != invoice.RechargeAccounts.Where(a => a.Direction == RechargeAccount.CreditDebit.Debit).Sum(a => a.Amount))
                {
                    throw new ArgumentException("Total of debit recharge accounts must equal invoice total when supplied.", nameof(model.RechargeAccounts));
                }
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

            //Possibly we want to validate the recharge accounts again before sending?
            if(invoice.Type == Invoice.InvoiceTypes.Recharge)
            {
                foreach (var ra in invoice.RechargeAccounts)
                {
                    var validationModel = await _aggieEnterpriseService.IsRechargeAccountValid(ra.FinancialSegmentString, ra.Direction);
                    if (!validationModel.IsValid)
                    {
                        //This will throw an uncontrolled exception, but that is probably better than sending an invoice with invalid accounts.
                        throw new ArgumentException($"Recharge account '{ra.FinancialSegmentString}' is not valid");
                    }
                }
            }

            await _emailService.SendInvoice(invoice, model.ccEmails, model.bccEmails);

            if (invoice.Type == Invoice.InvoiceTypes.Recharge)
            {
                //We don't want to change the status of these just because we resend
                if(invoice.Status == Invoice.StatusCodes.Draft || invoice.Status == Invoice.StatusCodes.Rejected)
                {
                    invoice.Status = Invoice.StatusCodes.Sent;
                }
            }
            else
            {
                invoice.Status = Invoice.StatusCodes.Sent;
            }
                
            invoice.Sent = true;
            invoice.SentAt = DateTime.UtcNow;
        }

        public async Task SendReceipt(Invoice invoice, PaymentEvent payment, SendInvoiceModel model)
        {
            //This is probably never called. It isn't listed in the interface. Beware if it gets implemented.
            await _emailService.SendReceipt(invoice, payment);

            invoice.Status = Invoice.StatusCodes.Sent; //This might be wrong for non-recharge invoices? But maybe not. I think this should only be set if the invoice is sent.
            invoice.Sent = true;
            invoice.SentAt = DateTime.UtcNow;
        }

        public async Task SendRechargeReceipt(Invoice invoice)
        {
            //Uncomment if we wanted to notify all financial approver vs. just those who approved.
            //SendApprovalModel people = await GetInvoiceApprovers(invoice);

            SendApprovalModel people = new SendApprovalModel();
            var approvers = new List<EmailRecipient>();
            foreach(var ra in invoice.RechargeAccounts.Where(a => a.Direction == RechargeAccount.CreditDebit.Debit))
            {
                if(ra.ApprovedByKerb == null || ra.ApprovedByKerb == "System")
                {
                    continue;
                }
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.CampusKerberos == ra.ApprovedByKerb);
                if ((user != null && user.Email != null))
                {
                    approvers.Add(new EmailRecipient()
                    {
                        Email = user.Email,
                        Name = ra.ApprovedByName
                    });
                }
            }

            people.emails = approvers.DistinctBy(a => a.Email.ToLower()).ToArray();

            await _emailService.SendRechargeReceipt(invoice, people);
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

        public async Task<SendApprovalModel> SendFinancialApproverEmail(Invoice invoice, SendApprovalModel model)
        {
            if (invoice.Type != Invoice.InvoiceTypes.Recharge)
            {
                return null;
            }

            if(model == null)
            {
                model = await GetInvoiceApprovers(invoice);
            }

            //The cc emails are actually going to be the to emails in this case.
            //We might want to change it a little to have the names as well.
            await _emailService.SendFinancialApprove(invoice, model);

            return model;
        }

        private async Task<SendApprovalModel> GetInvoiceApprovers(Invoice invoice)
        {
            if(invoice.RechargeAccounts == null || !invoice.RechargeAccounts.Any(a => a.Direction == RechargeAccount.CreditDebit.Debit))
            {
                throw new ArgumentException("Invoice has no debit recharge accounts to get approvers from.", nameof(invoice));
            }

            var approvers = new List<Approver>();
            foreach(var ra in invoice.RechargeAccounts.Where(a => a.Direction == RechargeAccount.CreditDebit.Debit))
            {
                var validationResult = await _aggieEnterpriseService.IsRechargeAccountValid(ra.FinancialSegmentString, ra.Direction);
                if(validationResult.Approvers != null)
                {
                    approvers.AddRange(validationResult.Approvers);
                }
            }
            var approverDistinct = approvers
                .Where(a => !string.IsNullOrWhiteSpace(a.Email))
                .DistinctBy(a => a.Email!.ToLower())
                .ToList();

            var emails = new List<EmailRecipient>();
            foreach (var approver in approverDistinct)
            {
                emails.Add(new EmailRecipient()
                {
                    Email = approver.Email,
                    Name = approver.Name
                });
            }

            return new SendApprovalModel()
            {
                emails = emails.ToArray(),
                bccEmails = "" //TODO: Add any BCC emails if needed. Note customer is CC'd by default in the service.
            };

        }

        public async Task<Invoice> CopyInvoice(Invoice invoiceToCopy, Team team, User user)
        {            
            //This assumes the team or other validation has happened
            var invoice = new Invoice()
            {
                LinkId = null,
                DraftCount = 1,                
                CustomerName = invoiceToCopy.CustomerName,
                CustomerAddress = invoiceToCopy.CustomerAddress,
                CustomerEmail = invoiceToCopy.CustomerEmail,
                CustomerCompany = invoiceToCopy.CustomerCompany,
                Memo = invoiceToCopy.Memo,
                TaxPercent = invoiceToCopy.TaxPercent,
                DueDate = invoiceToCopy.DueDate,
                Status = Invoice.StatusCodes.Draft,
                Coupon = null, //Don't think we want any coupon as this may have been entered by customer.
                ManualDiscount = 0.0m,
                Account = invoiceToCopy.Account, //Verify if this is still valid?
                Attachments = null,
                Team = invoiceToCopy.Team,
                Sent = false,
                SentAt = null,
                Paid = false,
                PaidAt = null,
                Refunded = false,
                RefundedAt = null,
                PaymentType = null,
                PaymentProcessorId = null,
                KfsTrackingNumber = null,
                CreatedAt = DateTime.UtcNow,
                Deleted = false,
                DeletedAt = null,
                Type = invoiceToCopy.Type,
            };

            if(invoice.Account != null)
            {
                if(team.Accounts.Where(a => a.IsActive && a.Id == invoice.Account.Id).Any())
                {
                    // Account is valid for the team
                }
                else
                {
                    //If they don't have any active accounts that are default, BOOM!
                    invoice.Account = team.Accounts.Where(a => a.IsActive && a.IsActive).Single();

                    var history = new History()
                    {
                        Type = HistoryActionTypes.InvoiceCopied.TypeCode,
                        ActionDateTime = DateTime.UtcNow,
                        Actor = user.Name,
                        Data = $"Account changed with Invoice copy.",
                    };
                    invoice.History.Add(history);
                }
            }


            //Line items
            foreach(var item in invoiceToCopy.Items)
            {
                var newItem = new LineItem()
                {
                    Description = item.Description,
                    Quantity = item.Quantity,
                    Amount = item.Amount,
                    Total = item.Total,
                    TaxExempt = item.TaxExempt,                    
                };
                invoice.Items.Add(newItem);
            }
            //Recharge accounts
            if(invoiceToCopy.RechargeAccounts != null)
            {
                foreach(var ra in invoiceToCopy.RechargeAccounts)
                {
                    var newRa = new RechargeAccount()
                    {
                        Direction = ra.Direction,
                        FinancialSegmentString = ra.FinancialSegmentString,
                        Amount = ra.Amount,
                        Percentage = ra.Percentage,
                        EnteredByKerb = user.CampusKerberos, 
                        EnteredByName = user.Name,
                        ApprovedByKerb = null,
                        ApprovedByName = null,
                        Notes = ra.Notes,
                    };                    

                    invoice.RechargeAccounts.Add(newRa);
                }
            }
            //Possibly attachments, but probably more pain then it's worth to copy those.

            //Recalculate totals.
            invoice.UpdateCalculatedValues();

            await _dbContext.Invoices.AddAsync(invoice);
            return invoice;

        }
    }

    public interface IInvoiceService
    {
        Task<IReadOnlyList<Invoice>> CreateInvoices(CreateInvoiceModel model, Team team);

        Task<Invoice> UpdateInvoice(Invoice invoice, EditInvoiceModel model);

        Task<Invoice> CopyInvoice(Invoice invoice, Team team, User user);

        Task SendInvoice(Invoice invoice, SendInvoiceModel model);

        Task SendRechargeReceipt(Invoice invoice);

        Task<SendApprovalModel> SendFinancialApproverEmail(Invoice invoice, SendApprovalModel model);

        Task RefundInvoice(Invoice invoice, PaymentEvent payment, string refundReason, User user);

        string SetInvoiceKey(Invoice invoice);
    }
}
