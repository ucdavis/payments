using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Models.Configuration;
using Serilog;
using SparkPost;

namespace Payments.Core.Services
{
    public interface IEmailService
    {
        Task SendInvoice(Invoice invoice);
    }

    public class SparkpostEmailService : IEmailService
    {
        private readonly SparkpostSettings _emailSettings;
        private readonly ApplicationDbContext _dbContext;

        public SparkpostEmailService(IOptions<SparkpostSettings> emailSettings, ApplicationDbContext dbContext)
        {
            _emailSettings = emailSettings.Value;
            _dbContext = dbContext;
        }

        public async Task SendInvoice(Invoice invoice)
        {
            var client = GetClient();

            // build substitution data
            var data = new Dictionary<string, object>()
            {
                { "id", invoice.Id },
                { "customer", new { email = invoice.CustomerEmail, name = invoice.CustomerName } },
                { "discount", invoice.Discount.ToString("F2") },
                { "taxAmount", invoice.TaxAmount.ToString("F2") },
                { "taxPercent", (invoice.TaxPercent * 100).ToString("F") },
                { "total", invoice.Total.ToString("F2") },
                { "items", invoice.Items.Select(i => new { description = i.Description, quantity = i.Quantity, total = i.Total.ToString("F2") }) },
                { "memo", invoice.Memo },
            };

            var transmission = new Transmission();
            transmission.Content.TemplateId = "invoice-send";
            transmission.Recipients = new List<Recipient>()
            {
                new Recipient() { Address = new Address(invoice.CustomerEmail, invoice.CustomerName), SubstitutionData = data },
            };

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
