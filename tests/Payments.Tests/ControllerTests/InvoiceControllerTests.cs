using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Moq;
using payments.Tests.Helpers;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Models.History;
using Payments.Core.Services;
using Payments.Mvc.Controllers;
using Shouldly;
using TestHelpers.Helpers;
using Xunit;

namespace payments.Tests.ControllerTests
{
    [Trait("Category", "ControllerTests")]
    public class InvoiceControllerTests
    {
        public Mock<ApplicationDbContext> MockDbContext { get; set; }
        public Mock<HttpContext> MockHttpContext { get; set; }
        public Mock<IEmailService> MockEmailService { get; set; }
        public Mock<FakeApplicationUserManager> MockUserManager { get; set; }
        //public Mock<ClaimsPrincipal> MockClaimsPrincipal { get; set; }

        //Setup Data
        public List<Invoice> InvoiceData { get; set; }
        public List<User> UserData { get; set; }

        //Controller
        public InvoicesController Controller{ get; set; }

        public InvoiceControllerTests()
        {
            var mockDataProvider = new Mock<SessionStateTempDataProvider>();

            MockDbContext = new Mock<ApplicationDbContext>();
            MockEmailService = new Mock<IEmailService>();
            MockUserManager = new Mock<FakeApplicationUserManager>();

            //MockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            MockHttpContext = new Mock<HttpContext>();

            //Default Data
            UserData = new List<User>();
            for (int i = 0; i < 5; i++)
            {
                var user = CreateValidEntities.User(i + 1);
                UserData.Add(user);
            }

            InvoiceData = new List<Invoice>();
            for (int i = 0; i < 5; i++)
            {
                var invoice = CreateValidEntities.Invoice(i + 1);
                invoice.Status = Invoice.StatusCodes.Paid;
                InvoiceData.Add(invoice);
            }

            InvoiceData[1].Status = Invoice.StatusCodes.Sent;

            //Setups
            MockDbContext.Setup(a => a.Invoices).Returns(InvoiceData.AsQueryable().MockAsyncDbSet().Object);
            MockUserManager.Setup(a => a.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(UserData[1]);

            var routeData = new RouteData();
            routeData.Values.Add( "team", "testSlug" );
            Controller = new InvoicesController(MockUserManager.Object, MockDbContext.Object, MockEmailService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = MockHttpContext.Object,
                    RouteData = routeData
                },
                TempData = new TempDataDictionary(MockHttpContext.Object, mockDataProvider.Object),
            };
        }

        [Fact]
        public async Task TestUnlockUpdatesInvoiceWhenHasNotBeenPaid()
        {
            // Arrange
            InvoiceData[0].Status = Invoice.StatusCodes.Sent;
            InvoiceData[0].Sent = true;
            InvoiceData[0].SentAt = DateTime.UtcNow;
            InvoiceData[0].LinkId = "FakeLink";

            // Act
            var controllerResult = await Controller.Unlock(1);

            // Assert	
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            InvoiceData[0].Status.ShouldBe(Invoice.StatusCodes.Draft);
            InvoiceData[0].Sent.ShouldBeFalse();
            InvoiceData[0].SentAt.ShouldBeNull();
            InvoiceData[0].LinkId.ShouldBeNull();

            InvoiceData[0].History.ShouldNotBeNull();
            InvoiceData[0].History.Count.ShouldBe(1);
            InvoiceData[0].History[0].Actor.FirstName.ShouldBe("FirstName2");
            InvoiceData[0].History[0].Type.ShouldBe(HistoryActionTypes.InvoiceUnlocked.TypeCode);
            InvoiceData[0].History[0].ActionDateTime.ShouldNotBeNull();
        }

        [Fact]
        public async Task TestUnlockDoesNotUpdateInvoiceWhenHasBeenPaid()
        {
            // Arrange
            var compareDate = new DateTime(2018, 01, 20);
            InvoiceData[0].Status = Invoice.StatusCodes.Paid;
            InvoiceData[0].Sent = true;
            InvoiceData[0].SentAt = compareDate;
            InvoiceData[0].LinkId = "FakeLink";

            // Act
            var controllerResult = await Controller.Unlock(1);

            // Assert	
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            InvoiceData[0].Status.ShouldBe(Invoice.StatusCodes.Paid);
            InvoiceData[0].Sent.ShouldBeTrue();
            InvoiceData[0].SentAt.ShouldBe(compareDate);
            InvoiceData[0].LinkId.ShouldBe("FakeLink");

            InvoiceData[0].History.ShouldNotBeNull();
            InvoiceData[0].History.Count.ShouldBe(0);
        }
    }

    [Trait("Category", "Controller Reflection")]
    public class InvoiceControllerReflectionTests
    {
        //TODO: Reflection
    }
}
