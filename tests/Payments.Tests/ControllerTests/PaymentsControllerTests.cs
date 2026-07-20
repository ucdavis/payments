using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using payments.Tests.Helpers;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Models.History;
using Payments.Core.Resources;
using Payments.Core.Services;
using Payments.Emails;
using Payments.Mvc.Controllers;
using Payments.Mvc.Models.Configuration;
using Payments.Mvc.Models.PaymentViewModels;
using Payments.Mvc.Services;
using Shouldly;
using TestHelpers.Helpers;
using Xunit;

namespace payments.Tests.ControllerTests
{
    [Trait("Category", "ControllerTests")]
    public class PaymentsControllerTests
    {
        [Fact]
        public async Task Pay_ZeroBalanceSentCreditCard_MarksInvoiceCompletedWithoutCyberSourceRequest()
        {
            var invoice = new Invoice
            {
                Id = 1,
                LinkId = "zero-balance",
                Status = Invoice.StatusCodes.Sent,
                Sent = true,
                Type = Invoice.InvoiceTypes.CreditCard,
                CustomerName = "Test Customer",
                CustomerEmail = "customer@example.com",
                Team = new Team { Name = "Test Team", Slug = "test-team" },
                Coupon = new Coupon { DiscountAmount = 10 },
                Items = new List<LineItem>
                {
                    new LineItem { Amount = 10, Quantity = 1, Total = 10 },
                },
            };

            var dbContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            dbContext
                .Setup(context => context.Invoices)
                .Returns(new List<Invoice> { invoice }.AsQueryable().MockAsyncDbSet().Object);
            dbContext
                .Setup(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var dataSigningService = new Mock<IDataSigningService>();
            var controller = new PaymentsController(
                dbContext.Object,
                dataSigningService.Object,
                new Mock<INotificationService>().Object,
                new Mock<IStorageService>().Object,
                Options.Create(new CyberSourceSettings()),
                new Mock<IEmailService>().Object,
                new Mock<FakeApplicationUserManager>().Object);

            var result = await controller.Pay(invoice.LinkId);

            var viewResult = result.ShouldBeOfType<ViewResult>();
            var model = viewResult.Model.ShouldBeOfType<PaymentInvoiceViewModel>();
            invoice.Status.ShouldBe(Invoice.StatusCodes.Completed);
            invoice.Paid.ShouldBeTrue();
            invoice.PaidAt.ShouldNotBeNull();
            invoice.ManualDiscount.ShouldBe(10);
            invoice.PaymentType.ShouldBe(PaymentTypes.Coupon);
            invoice.History.ShouldContain(history => history.Type == HistoryActionTypes.MarkPaid.TypeCode);
            model.Paid.ShouldBeTrue();
            model.Total.ShouldBe(0);
            model.PaymentDictionary.ShouldBeNull();
            dbContext.Verify(
                context => context.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
            dataSigningService.Verify(
                service => service.Sign(It.IsAny<IDictionary<string, string>>()),
                Times.Never);
        }
    }
}
