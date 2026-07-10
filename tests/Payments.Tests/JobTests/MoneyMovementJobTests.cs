using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Jobs;
using Payments.Core.Models.Configuration;
using Payments.Core.Models.Sloth;
using Payments.Core.Models.Validation;
using Payments.Core.Services;
using Serilog;
using TestHelpers.Helpers;
using Xunit;

namespace payments.Tests.JobTests
{
    public class MoneyMovementJobTests
    {
        [Theory]
        [InlineData(true, "override-chart-string")]
        [InlineData(false, "normal-chart-string")]
        public async Task FindBankReconcileTransactions_UsesValidOverrideAndFallsBackWhenInvalid(
            bool overrideIsValid,
            string expectedChartString)
        {
            var defaultAccount = new FinancialAccount
            {
                IsDefault = true,
                IsActive = true,
                FinancialSegmentString = "default-chart-string",
            };
            var normalAccount = new FinancialAccount
            {
                IsActive = true,
                FinancialSegmentString = "normal-chart-string",
            };
            var team = new Team
            {
                Name = "Test Team",
                Slug = "test-team",
                Accounts = new List<FinancialAccount> { defaultAccount, normalAccount },
            };
            var invoice = new Invoice
            {
                Id = 1,
                Status = Invoice.StatusCodes.Paid,
                Type = Invoice.InvoiceTypes.CreditCard,
                PaymentProcessorId = "processor-id",
                Team = team,
                Account = normalAccount,
                RechargeAccounts = new List<RechargeAccount>
                {
                    new RechargeAccount
                    {
                        Direction = RechargeAccount.CreditDebit.Credit,
                        FinancialSegmentString = "override-chart-string",
                    },
                },
                Items = new List<LineItem>
                {
                    new LineItem
                    {
                        Amount = 10,
                        Quantity = 1,
                        Total = 10,
                        Description = "Test item",
                    },
                },
            };
            invoice.UpdateCalculatedValues();

            var dbContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            dbContext
                .Setup(context => context.Invoices)
                .Returns(new List<Invoice> { invoice }.AsQueryable().MockAsyncDbSet().Object);

            var slothService = new Mock<ISlothService>();
            slothService
                .Setup(service => service.GetTransactionsByProcessorId("processor-id", false))
                .ReturnsAsync(new List<Transaction>
                {
                    new Transaction
                    {
                        Status = "Completed",
                        MerchantTrackingNumber = "merchant-id",
                        KfsTrackingNumber = "kfs-id",
                    },
                });

            CreateTransaction createdTransaction = null;
            slothService
                .Setup(service => service.CreateTransaction(It.IsAny<CreateTransaction>(), false))
                .Callback<CreateTransaction, bool>((transaction, _) => createdTransaction = transaction)
                .ReturnsAsync(new CreateSlothTransactionResponse { Id = "sloth-id" });

            var aggieEnterpriseService = new Mock<IAggieEnterpriseService>();
            aggieEnterpriseService
                .Setup(service => service.IsAccountValid("override-chart-string", true))
                .ReturnsAsync(new AccountValidationModel
                {
                    IsValid = overrideIsValid,
                    ChartString = "override-chart-string",
                });

            var job = new MoneyMovementJob(
                dbContext.Object,
                slothService.Object,
                aggieEnterpriseService.Object,
                new Mock<INotificationService>().Object,
                Options.Create(new FinanceSettings
                {
                    ClearingFinancialSegmentString = "clearing-chart-string",
                    FeeFinancialSegmentString = "fee-chart-string",
                }),
                Options.Create(new PaymentsApiSettings()));

            await job.FindBankReconcileTransactions(new Mock<ILogger>().Object);

            Assert.NotNull(createdTransaction);
            var incomeTransfer = createdTransaction.Transfers.Single(
                transfer => transfer.Direction == Transfer.CreditDebit.Credit
                            && transfer.Description == "Funds Distribution");
            Assert.Equal(expectedChartString, incomeTransfer.FinancialSegmentString);
            aggieEnterpriseService.Verify(
                service => service.IsAccountValid("override-chart-string", true),
                Times.Once);
        }
    }
}