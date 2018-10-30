using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Payments.Core.Domain;
using Payments.Core.Models.Configuration;
using Serilog;
using SparkPost;

namespace Payments.Core.Services
{
    public interface IEmailService
    {
        Task SendInvoice(Invoice invoice);

        Task SendTaxReport(Recipient recipient, Attachment report);
    }

    public class SparkpostEmailService : IEmailService
    {
        private readonly SparkpostSettings _emailSettings;

        public SparkpostEmailService(IOptions<SparkpostSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendInvoice(Invoice invoice)
        {
            var client = GetClient();

            // build substitution data
            var data = new Dictionary<string, object>()
            {
                { "baseUrl", _emailSettings.BaseUrl },
                { "id", invoice.Id },
                { "linkid", invoice.LinkId },
                { "customer", new { email = invoice.CustomerEmail, name = invoice.CustomerName } },
                { "discount", invoice.Discount.ToString("F2") },
                { "taxAmount", invoice.TaxAmount.ToString("F2") },
                { "taxPercent", (invoice.TaxPercent * 100).ToString("F") },
                { "total", invoice.Total.ToString("F2") },
                { "items", invoice.Items.Select(i => new { description = i.Description, quantity = i.Quantity, total = i.Total.ToString("F2") }) },
                { "memo", invoice.Memo },
            };

            // build email
            var transmission = new Transmission();
            transmission.Content.TemplateId = "invoice-send";
            transmission.Recipients = new List<Recipient>()
            {
                new Recipient() { Address = new Address(invoice.CustomerEmail, invoice.CustomerName), SubstitutionData = data },
            };

            // ship it
            var result = await client.Transmissions.Send(transmission);
            Log.ForContext("result", result).Information("Sent Email");
        }

        public async Task SendTaxReport(Recipient recipient, Attachment report)
        {
            var client = GetClient();

            // build email
            var transmission = new Transmission();
            transmission.Content.Subject = "New Report Ready";
            transmission.Content.From = new Address("donotreply@payments-mail.ucdavis.edu", "UC Davis Payments");
            transmission.Content.Text = "New report ready";
            transmission.Content.Attachments = new List<Attachment>(){ report };
            transmission.Recipients = new List<Recipient>() { recipient };

            // ship it
            var result = await client.Transmissions.Send(transmission);
            Log.ForContext("result", result).Information("Sent Email");
        }

        private Client GetClient()
        {
            var client = new Client(_emailSettings.ApiKey);
            return client;
        }
    }
}
