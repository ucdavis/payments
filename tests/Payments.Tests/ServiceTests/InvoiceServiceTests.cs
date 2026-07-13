using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Models.Invoice;
using Payments.Core.Models.Validation;
using Payments.Core.Services;
using Payments.Emails;
using Payments.Mvc.Services;
using TestHelpers.Helpers;
using Xunit;

namespace payments.Tests.ServiceTests
{
    public class InvoiceServiceTests
    {
        [Fact]
        public async Task CreateInvoices_CopiesExternalDetailsToCreatedInvoices()
        {
            var team = new Team { Id = 1 };
            var account = new FinancialAccount
            {
                Id = 2,
                Team = team,
                IsActive = true,
            };
            var dbContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            dbContext
                .Setup(context => context.FinancialAccounts)
                .Returns(new List<FinancialAccount> { account }.AsQueryable().MockAsyncDbSet().Object);
            dbContext
                .Setup(context => context.Invoices)
                .Returns(new Mock<DbSet<Invoice>>().Object);

            var service = new InvoiceService(
                dbContext.Object,
                new Mock<IEmailService>().Object,
                new Mock<IAggieEnterpriseService>().Object);
            var model = new CreateInvoiceModel
            {
                AccountId = account.Id,
                Customers = new List<CreateInvoiceCustomerModel>
                {
                    new CreateInvoiceCustomerModel { Email = "customer@example.com" },
                },
                Items = new List<CreateInvoiceItemModel>
                {
                    new CreateInvoiceItemModel { Description = "Item", Quantity = 1, Amount = 10 },
                },
                Attachments = new List<CreateInvoiceAttachmentModel>(),
                ExternalIdentifier = "External System",
                ExternalId = "record-123",
                ExternalLink = "https://example.com/records/record-123",
            };

            var invoices = await service.CreateInvoices(model, team);

            var invoice = Assert.Single(invoices);
            Assert.Equal(model.ExternalIdentifier, invoice.ExternalIdentifier);
            Assert.Equal(model.ExternalId, invoice.ExternalId);
            Assert.Equal(model.ExternalLink, invoice.ExternalLink);
        }

        [Fact]
        public async Task CreateInvoices_InvalidRechargeAccount_IdentifiesRechargeAccountsParameter()
        {
            var dbContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            dbContext
                .Setup(context => context.FinancialAccounts)
                .Returns(new List<FinancialAccount>().AsQueryable().MockAsyncDbSet().Object);

            var aggieEnterpriseService = new Mock<IAggieEnterpriseService>();
            aggieEnterpriseService
                .Setup(service => service.IsRechargeAccountValid(
                    "invalid-chart-string", RechargeAccount.CreditDebit.Credit, true))
                .ReturnsAsync(new AccountValidationModel { IsValid = false });

            var service = new InvoiceService(
                dbContext.Object,
                new Mock<IEmailService>().Object,
                aggieEnterpriseService.Object);
            var model = new CreateInvoiceModel
            {
                Type = Invoice.InvoiceTypes.Recharge,
                RechargeAccounts = new List<RechargeAccount>
                {
                    new RechargeAccount
                    {
                        Direction = RechargeAccount.CreditDebit.Credit,
                        FinancialSegmentString = "invalid-chart-string",
                        Amount = 1,
                    },
                },
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateInvoices(model, new Team { Id = 1 }));

            Assert.Equal(nameof(model.RechargeAccounts), exception.ParamName);
        }
    }
}
