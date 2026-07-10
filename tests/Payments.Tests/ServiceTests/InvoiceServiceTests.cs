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
