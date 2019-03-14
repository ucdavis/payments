using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Mjml.AspNetCore;
using Payments.Core.Domain;
using Payments.Core.Models.Configuration;
using Payments.Emails.Models;
using RazorLight;
using Serilog;
using SparkPost;

namespace Payments.Emails
{
    public interface IEmailService
    {
        Task SendInvoice(Invoice invoice, string ccEmails, string bccEmails);

        Task SendReceipt(Invoice invoice, PaymentEvent payment);
    }

    public class SparkpostEmailService : IEmailService
    {
        private readonly SparkpostSettings _emailSettings;

        private readonly IMjmlServices _mjmlServices;

        private readonly Address FromAddress = new Address("donotreply@payments-mail.ucdavis.edu", "UC Davis Payments");

        public SparkpostEmailService(IOptions<SparkpostSettings> emailSettings, IMjmlServices mjmlServices)
        {
            _mjmlServices = mjmlServices;

            _emailSettings = emailSettings.Value;
        }

        public async Task SendInvoice(Invoice invoice, string ccEmails, string bccEmails)
        {
            var client = GetClient();

            dynamic viewbag = new ExpandoObject();
            viewbag.BaseUrl = _emailSettings.BaseUrl;
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
            var transmission = new Transmission
            {
                Content =
                {
                    From = FromAddress,
                    Html = mjml.Html,
                    Subject = $"New invoice from {invoice.Team.Name}",
                },
                Recipients = new List<Recipient>()
                {
                    new Recipient() {Address = new Address(invoice.CustomerEmail, invoice.CustomerName)},
                }
            };

            // add cc
            if (!string.IsNullOrWhiteSpace(ccEmails))
            {
                var splitEmails = ccEmails.Split(';');
                var ccRecipients = splitEmails.Select(e => new Recipient()
                    { Address = new Address(e), Type = RecipientType.CC });
                foreach (var ccRecipient in ccRecipients)
                {
                    transmission.Recipients.Add(ccRecipient);
                }
            }

            // add bcc
            if (!string.IsNullOrWhiteSpace(bccEmails))
            {
                var splitEmails = bccEmails.Split(';');
                var bccRecipients = splitEmails.Select(e => new Recipient()
                    { Address = new Address(e), Type = RecipientType.BCC });
                foreach (var bccRecipient in bccRecipients)
                {
                    transmission.Recipients.Add(bccRecipient);
                }
            }

            // ship it
            var result = await client.Transmissions.Send(transmission);
            Log.ForContext("result", result).Information("Sent Email");
        }

        public async Task SendReceipt(Invoice invoice, PaymentEvent payment)
        {
            var client = GetClient();

            dynamic viewbag = new ExpandoObject();
            viewbag.BaseUrl = _emailSettings.BaseUrl;
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
            MjmlResponse mjml = await _mjmlServices.Render(prehtml);

            // build email
            var transmission = new Transmission
            {
                Content =
                {
                    From = FromAddress,
                    Html = mjml.Html,
                    Subject = $"Receipt from {invoice.Team.Name}",
                },
                Recipients = new List<Recipient>()
                {
                    new Recipient() {Address = new Address(invoice.CustomerEmail, invoice.CustomerName)},
                }
            };

            // add cc if the billing email is different
            if (!string.Equals(invoice.CustomerEmail, payment.BillingEmail, StringComparison.OrdinalIgnoreCase))
            {
                var ccName = $"{payment.BillingFirstName} {payment.BillingLastName}";
                var ccRecipient = new Recipient()
                {
                    Address = new Address(payment.BillingEmail, ccName),
                    Type = RecipientType.CC,
                };
                transmission.Recipients.Add(ccRecipient);
            }

            // ship it
            var result = await client.Transmissions.Send(transmission);
            Log.ForContext("result", result).Information("Sent Email");
        }

        private static RazorLightEngine GetRazorEngine()
        {
            var engine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(SparkpostEmailService))
                .UseMemoryCachingProvider()
                .Build();

            return engine;
        }

        private Client GetClient()
        {
            var client = new Client(_emailSettings.ApiKey);
            return client;
        }
    }
}
