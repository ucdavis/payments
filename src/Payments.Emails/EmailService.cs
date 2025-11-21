using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Mjml.AspNetCore;
using Payments.Core.Domain;
using Payments.Core.Models.Configuration;
using Payments.Core.Models.History;
using Payments.Emails.Models;
using Serilog;
using System.Net;
using System.Net.Mail;
using Razor.Templating.Core;
using Payments.Core.Models.Invoice;

namespace Payments.Emails
{
    public interface IEmailService
    {
        Task SendInvoice(Invoice invoice, string ccEmails, string bccEmails);

        Task SendFinancialApprove(Invoice invoice, SendApprovalModel approvalModel);

        Task SendReceipt(Invoice invoice, PaymentEvent payment);

        Task SendRechargeReceipt(Invoice invoice, SendApprovalModel approvalModel);

        Task SendNewTeamMemberNotice(Team team, User user, TeamRole role);

        Task SendRefundRequest(Invoice invoice, PaymentEvent payment, string refundReason, User user);
    }

    public class SparkpostEmailService : IEmailService
    {
        private readonly SparkpostSettings _sparkpostSettings;

        private readonly IMjmlServices _mjmlServices;

        private readonly SmtpClient _client;

        private readonly MailAddress _fromAddress = new MailAddress("donotreply@payments-mail.ucdavis.edu", "UC Davis Payments");

        private readonly MailAddress _refundAddress;

        private readonly FinanceSettings _financeSettings;

        public SparkpostEmailService(IOptions<SparkpostSettings> sparkpostSettings, IMjmlServices mjmlServices, IOptions<FinanceSettings> financeSettings)
        {
            _mjmlServices = mjmlServices;
            _financeSettings = financeSettings.Value;

            _sparkpostSettings = sparkpostSettings.Value;

            _client = new SmtpClient(_sparkpostSettings.Host, _sparkpostSettings.Port) { Credentials = new NetworkCredential(_sparkpostSettings.UserName, _sparkpostSettings.Password), EnableSsl = true };

            _refundAddress = new MailAddress(_sparkpostSettings.RefundAddress, "CAES UC Davis Refunds");
        }

        public async Task SendInvoice(Invoice invoice, string ccEmails, string bccEmails)
        {
            var viewbag = GetViewData();
            viewbag["Team"] = invoice.Team;

            var model = new InvoiceViewModel
            {
                Invoice = invoice,
            };

            // add model data to email
            var prehtml = await RazorTemplateEngine.RenderAsync("/Views/Invoice.cshtml", model, viewbag);

            // convert email to real html
            MjmlResponse mjml = await _mjmlServices.Render(prehtml);

            // build email
            using (var message = new MailMessage { From = _fromAddress, Subject = $"New invoice from {invoice.Team.Name}" })
            {
                message.Body = mjml.Html;
                message.IsBodyHtml = true;
                message.To.Add(new MailAddress(invoice.CustomerEmail, invoice.CustomerName));

                // add cc
                if (!string.IsNullOrWhiteSpace(ccEmails))
                {
                    ccEmails.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => new MailAddress(e))
                        .ToList()
                        .ForEach(e => message.CC.Add(e));
                }

                // add bcc
                if (!string.IsNullOrWhiteSpace(bccEmails))
                {
                    bccEmails.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => new MailAddress(e))
                        .ToList()
                        .ForEach(e => message.Bcc.Add(e));
                }

                // ship it
                await _client.SendMailAsync(message);
                Log.Information("Sent Email");
            }
        }

        public async Task SendReceipt(Invoice invoice, PaymentEvent payment)
        {
            var viewbag = GetViewData();
            viewbag["Team"] = invoice.Team;

            var model = new ReceiptViewModel
            {
                Invoice = invoice,
                Payment = payment,
            };

            // add model data to email
            var prehtml = await RazorTemplateEngine.RenderAsync("/Views/Receipt.cshtml", model, viewbag);

            // convert email to real html
            var mjml = await _mjmlServices.Render(prehtml);

            // build email
            using (var message = new MailMessage { From = _fromAddress, Subject = $"Receipt from {invoice.Team.Name}" })
            {
                message.Body = mjml.Html;
                message.IsBodyHtml = true;
                message.To.Add(new MailAddress(invoice.CustomerEmail, invoice.CustomerName));

                // add cc if the billing email is different
                if (!string.Equals(invoice.CustomerEmail, payment.BillingEmail, StringComparison.OrdinalIgnoreCase))
                {
                    var ccName = $"{payment.BillingFirstName} {payment.BillingLastName}";
                    message.CC.Add(new MailAddress(payment.BillingEmail, ccName));
                }

                // ship it
                await _client.SendMailAsync(message);
                Log.Information("Sent Email");
            }
        }

        public async Task SendNewTeamMemberNotice(Team team, User user, TeamRole role)
        {
            var viewbag = GetViewData();
            viewbag["Team"] = team;

            var model = new NewTeamMemberViewModel()
            {
                Team = team,
                User = user,
                Role = role
            };

            // add model data to email
            var prehtml = await RazorTemplateEngine.RenderAsync("/Views/NewTeamMember.cshtml", model, viewbag);

            // convert email to real html
            MjmlResponse mjml = await _mjmlServices.Render(prehtml);

            // build email
            using (var message = new MailMessage { From = _fromAddress, Subject = "UC Davis Payments Invitation" })
            {
                message.Body = mjml.Html;
                message.IsBodyHtml = true;
                message.To.Add(new MailAddress(user.Email, user.Name));

                // ship it
                await _client.SendMailAsync(message);
                Log.Information("Sent Email");
            }
        }

        public async Task SendRefundRequest(Invoice invoice, PaymentEvent payment, string refundReason, User user)
        {
            var viewbag = GetViewData();
            viewbag["Team"] = invoice.Team;
            viewbag["Slug"] = invoice.Team.Slug;

            var model = new RefundRequestViewModel()
            {
                Invoice = invoice,
                Payment = payment,
                RefundReason = refundReason,
                User = user,
            };

            // add model data to email
            var prehtml = await RazorTemplateEngine.RenderAsync("/Views/RefundRequest.cshtml", model, viewbag);

            // convert email to real html
            MjmlResponse mjml = await _mjmlServices.Render(prehtml);

            // build email
            using (var message = new MailMessage { From = _fromAddress, Subject = $"Refund Request from {invoice.Team.Name}" })
            {
                message.Body = mjml.Html;
                message.IsBodyHtml = true;
                message.To.Add(_refundAddress);

                // ship it
                await _client.SendMailAsync(message);
                Log.Information("Sent Email");
            }
        }


        public async Task SendFinancialApprove(Invoice invoice, SendApprovalModel approvalModel)
        {
            var viewbag = GetViewData();
            viewbag["Team"] = invoice.Team;
            viewbag["DaysToAutoApprove"] = _financeSettings.RechargeAutoApproveDays;

            var model = new InvoiceViewModel
            {
                Invoice = invoice,
            };

            // add model data to email
            var prehtml = await RazorTemplateEngine.RenderAsync("/Views/FinancialApprove.cshtml", model, viewbag);

            // convert email to real html
            MjmlResponse mjml = await _mjmlServices.Render(prehtml);

            // build email
            using (var message = new MailMessage { From = _fromAddress, Subject = $"Financial Approval recharge from CAES Payments team {invoice.Team.Name}" })
            {
                message.Body = mjml.Html;
                message.IsBodyHtml = true;

                foreach (var recipient in approvalModel.emails)
                {
                    message.To.Add(new MailAddress(recipient.Email, recipient.Name));
                }                

                message.CC.Add(new MailAddress(invoice.CustomerEmail, invoice.CustomerName));


                // add bcc
                if (!string.IsNullOrWhiteSpace(approvalModel.bccEmails))
                {
                    approvalModel.bccEmails.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => new MailAddress(e))
                        .ToList()
                        .ForEach(e => message.Bcc.Add(e));
                }

                // ship it
                await _client.SendMailAsync(message);
                Log.Information("Sent Email");
            }
        }

        private Dictionary<string, object> GetViewData()
        {
            return new Dictionary<string, object>{
                { "BaseUrl", _sparkpostSettings.BaseUrl },
            };
        }

        public async Task SendRechargeReceipt(Invoice invoice, SendApprovalModel approvalModel)
        {
            var viewbag = GetViewData();
            viewbag["Team"] = invoice.Team;

            var model = new ReceiptViewModel
            {
                Invoice = invoice,
                Payment = null,
            };

            // add model data to email
            var prehtml = await RazorTemplateEngine.RenderAsync("/Views/Receipt.cshtml", model, viewbag);

            // convert email to real html
            var mjml = await _mjmlServices.Render(prehtml);

            // build email
            using (var message = new MailMessage { From = _fromAddress, Subject = $"Receipt from {invoice.Team.Name}" })
            {
                message.Body = mjml.Html;
                message.IsBodyHtml = true;
                message.To.Add(new MailAddress(invoice.CustomerEmail, invoice.CustomerName));

                if (approvalModel != null)
                {
                    foreach (var recipient in approvalModel.emails)
                    {
                        message.CC.Add(new MailAddress(recipient.Email, recipient.Name));
                    }
                }


                // ship it
                await _client.SendMailAsync(message);
                Log.Information("Sent Email");
            }
        }
    }
}
