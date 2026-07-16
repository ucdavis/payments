using Microsoft.Extensions.DependencyInjection;
using Mjml.AspNetCore;
using Payments.Core.Domain;
using Payments.Emails.Models;
using Razor.Templating.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace payments.Tests.EmailTests
{
    public class FinancialApprovalRejectedEmailTests
    {
        [Fact]
        public async Task CustomerTemplate_RendersReasonAndPublicInvoiceLinks()
        {
            var invoice = CreateInvoice();
            var html = await Render(
                "/Views/FinancialApprovalRejectedCustomer.cshtml",
                invoice,
                "The debit account is incorrect.");

            Assert.Contains("The debit account is incorrect.", html);
            Assert.Contains("https://payments.example/recharge/pay/invoice-link", html);
            Assert.Contains("https://payments.example/download/invoice-link", html);
            Assert.DoesNotContain("<mj-", html);
        }

        [Fact]
        public async Task EditorsTemplate_RendersReasonAndInvoiceDetailsLink()
        {
            var invoice = CreateInvoice();
            var html = await Render(
                "/Views/FinancialApprovalRejectedEditors.cshtml",
                invoice,
                "The debit account is incorrect.");

            Assert.Contains("The debit account is incorrect.", html);
            Assert.Contains("https://payments.example/test-team/Invoices/Details/42", html);
            Assert.DoesNotContain("<mj-", html);
        }

        private static Invoice CreateInvoice()
        {
            return new Invoice
            {
                Id = 42,
                LinkId = "invoice-link",
                CustomerEmail = "customer@example.com",
                CustomerName = "Customer",
                Team = new Team
                {
                    Id = 7,
                    Name = "Test Team",
                    Slug = "test-team",
                    ContactEmail = "team@example.com",
                    ContactPhoneNumber = "530-555-0100",
                },
                Items = new List<LineItem>
                {
                    new LineItem
                    {
                        Description = "Service",
                        Quantity = 1,
                        Total = 25,
                    },
                },
            };
        }

        private static async Task<string> Render(string viewPath, Invoice invoice, string rejectionReason)
        {
            var viewData = new Dictionary<string, object>
            {
                { "BaseUrl", "https://payments.example" },
                { "Team", invoice.Team },
                { "Invoice", invoice },
            };
            var model = new FinancialApprovalRejectedViewModel
            {
                Invoice = invoice,
                RejectionReason = rejectionReason,
            };
            var mjmlMarkup = await RazorTemplateEngine.RenderAsync(viewPath, model, viewData);

            var services = new ServiceCollection();
            services.AddMjmlServices();
            using var serviceProvider = services.BuildServiceProvider();
            var renderer = serviceProvider.GetRequiredService<IMjmlServices>();
            var response = await renderer.Render(mjmlMarkup);

            return response.Html;
        }
    }
}
