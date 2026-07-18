using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using payments.Tests.Helpers;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Models.Invoice;
using Payments.Core.Models.Validation;
using Payments.Core.Services;
using Payments.Mvc.Controllers;
using Payments.Mvc.Services;
using Shouldly;
using TestHelpers.Helpers;
using Xunit;

namespace payments.Tests.ControllerTests
{
    [Trait("Category", "ControllerTests")]
    public class RechargeControllerTests
    {
        [Fact]
        public async Task Pay_AcceptsRechargeTotalRoundedToDisplayedCurrencyAmount()
        {
            var invoice = CreateInvoice();
            var (controller, dbContext, aggieEnterpriseService) = CreateController(invoice);
            var rechargeAccounts = CreateRechargeAccounts(9.35m);

            var result = await controller.Pay(invoice.LinkId, rechargeAccounts);

            result.ShouldBeOfType<OkResult>();
            aggieEnterpriseService.Verify(service => service.IsRechargeAccountValid(
                rechargeAccounts[0].FinancialSegmentString,
                RechargeAccount.CreditDebit.Debit,
                true), Times.Once);
            dbContext.Verify(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
            invoice.Status.ShouldBe(Invoice.StatusCodes.PendingApproval);
        }

        [Fact]
        public async Task Pay_RejectsRechargeTotalThatDiffersByOneCent()
        {
            var invoice = CreateInvoice();
            var (controller, dbContext, aggieEnterpriseService) = CreateController(invoice);

            var result = await controller.Pay(invoice.LinkId, CreateRechargeAccounts(9.34m));

            var badRequest = result.ShouldBeOfType<BadRequestObjectResult>();
            badRequest.Value.ShouldBe("The total of the recharge accounts does not match the invoice total. Please review and try again.");
            aggieEnterpriseService.Verify(service => service.IsRechargeAccountValid(
                It.IsAny<string>(),
                It.IsAny<RechargeAccount.CreditDebit>(),
                It.IsAny<bool>()), Times.Never);
            dbContext.Verify(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        private static Invoice CreateInvoice()
        {
            var invoice = new Invoice
            {
                Id = 1,
                LinkId = "recharge-link",
                Status = Invoice.StatusCodes.Sent,
                Team = new Team { Name = "Test Team", Slug = "test-team" }
            };
            invoice.Items.Add(new LineItem
            {
                Description = "Reported rounding case",
                Quantity = 0.06m,
                Amount = 155.83m,
                Total = 0.06m * 155.83m
            });
            invoice.UpdateCalculatedValues();
            invoice.CalculatedTotal.ShouldBe(9.3498m);

            return invoice;
        }

        private static RechargeAccount[] CreateRechargeAccounts(decimal amount)
        {
            return new[]
            {
                new RechargeAccount
                {
                    Direction = RechargeAccount.CreditDebit.Debit,
                    FinancialSegmentString = "valid-chart-string",
                    Amount = amount,
                    Percentage = 100m
                }
            };
        }

        private static (
            RechargeController controller,
            Mock<ApplicationDbContext> dbContext,
            Mock<IAggieEnterpriseService> aggieEnterpriseService) CreateController(Invoice invoice)
        {
            var dbContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            dbContext.Setup(context => context.Invoices)
                .Returns(new List<Invoice> { invoice }.AsQueryable().MockAsyncDbSet().Object);
            dbContext.Setup(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var aggieEnterpriseService = new Mock<IAggieEnterpriseService>();
            aggieEnterpriseService.Setup(service => service.IsRechargeAccountValid(
                    It.IsAny<string>(),
                    RechargeAccount.CreditDebit.Debit,
                    true))
                .ReturnsAsync((string chartString, RechargeAccount.CreditDebit _, bool __) =>
                    new AccountValidationModel
                    {
                        IsValid = true,
                        ChartString = chartString
                    });

            var user = new User
            {
                Id = "customer-user",
                CampusKerberos = "customer",
                Name = "Customer User",
                Email = "customer@example.com"
            };
            var userManager = new Mock<FakeApplicationUserManager>();
            userManager.Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var invoiceService = new Mock<IInvoiceService>();
            invoiceService.Setup(service => service.SendFinancialApproverEmail(
                    invoice,
                    It.IsAny<SendApprovalModel>()))
                .ReturnsAsync((Invoice _, SendApprovalModel model) => model);

            var controller = new RechargeController(
                dbContext.Object,
                aggieEnterpriseService.Object,
                userManager.Object,
                invoiceService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            return (controller, dbContext, aggieEnterpriseService);
        }
    }
}
