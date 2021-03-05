using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Mjml.AspNetCore;
using Payments.Core.Domain;
using Payments.Core.Models.Configuration;
using Payments.Core.Models.History;
using Payments.Emails.Models;
using RazorLight;
using Serilog;
using System.Net;
using System.Net.Mail;

namespace Payments.Emails
{
    public interface IEmailService
    {
        Task SendInvoice(Invoice invoice, string ccEmails, string bccEmails);

        Task SendReceipt(Invoice invoice, PaymentEvent payment);

        Task SendNewTeamMemberNotice(Team team, User user, TeamRole role);

        Task SendRefundRequest(Invoice invoice, PaymentEvent payment, string refundReason, User user);
    }

    public class SparkpostEmailService : IEmailService
    {
        private readonly SparkpostSettings _sparkpostSettings;

        private readonly IMjmlServices _mjmlServices;

        private readonly SmtpClient _client;

        private readonly MailAddress _fromAddress = new MailAddress("donotreply@payments-mail.ucdavis.edu", "UC Davis Payments");
#if DEBUG
        private readonly MailAddress _refundAddress = new MailAddress("jsylvestre@ucdavis.edu", "CAES UC Davis Refunds");
#else
        private readonly Address _refundAddress = new MailAddress("refunds@caes.ucdavis.edu", "CAES UC Davis Refunds");
#endif
        public SparkpostEmailService(IOptions<SparkpostSettings> emailSettings, IMjmlServices mjmlServices)
        {
            _mjmlServices = mjmlServices;

            _sparkpostSettings = emailSettings.Value;

            _client = new SmtpClient(_sparkpostSettings.Host, _sparkpostSettings.Port) { Credentials = new NetworkCredential(_sparkpostSettings.UserName, _sparkpostSettings.Password), EnableSsl = true };
        }

        public async Task SendInvoice(Invoice invoice, string ccEmails, string bccEmails)
        {
            dynamic viewbag = GetViewBag();
            viewbag.Team = invoice.Team;

            var model = new InvoiceViewModel
            {
                Invoice = invoice,
            };

            // add model data to email
            var engine = GetRazorEngine();
            var prehtml = await engine.CompileRenderAsync("Views.Invoice.cshtml", model, viewbag);

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
            dynamic viewbag = GetViewBag();
            viewbag.Team = invoice.Team;

            var model = new ReceiptViewModel
            {
                Invoice = invoice,
                Payment = payment,
            };

            // add model data to email
            var engine = GetRazorEngine();
            var prehtml = await engine.CompileRenderAsync("Views.Receipt.cshtml", model, viewbag);

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
            dynamic viewbag = GetViewBag();
            viewbag.Team = team;

            var model = new NewTeamMemberViewModel()
            {
                Team = team,
                User = user,
                Role = role
            };

            // add model data to email
            var engine = GetRazorEngine();
            var prehtml = await engine.CompileRenderAsync("Views.NewTeamMember.cshtml", model, viewbag);

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
            dynamic viewbag = GetViewBag();
            viewbag.Team = invoice.Team;
            viewbag.Slug = invoice.Team.Slug;

            var model = new RefundRequestViewModel()
            {
                Invoice = invoice,
                Payment = payment,
                RefundReason = refundReason,
                User = user,
            };

            // add model data to email
            var engine = GetRazorEngine();
            var prehtml = await engine.CompileRenderAsync("Views.RefundRequest.cshtml", model, viewbag);

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

        private static RazorLightEngine GetRazorEngine()
        {
            var engine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(SparkpostEmailService))
                .UseMemoryCachingProvider()
                .Build();

            return engine;
        }

        private ExpandoObject GetViewBag()
        {
            dynamic viewbag = new ExpandoObject();
            viewbag.BaseUrl = _sparkpostSettings.BaseUrl;
            return viewbag;
        }
    }
}
