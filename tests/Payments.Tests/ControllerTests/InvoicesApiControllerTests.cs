using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Payments.Core.Data;
using Payments.Core.Services;
using Payments.Mvc.Controllers;
using Payments.Mvc.Models.InvoiceApiViewModels;
using Payments.Mvc.Services;
using Shouldly;
using Xunit;

namespace payments.Tests.ControllerTests
{
    [Trait("Category", "ControllerTests")]
    public class InvoicesApiControllerTests
    {
        [Fact]
        public async Task GetByExternalRejectsBatchesAboveMaximumSize()
        {
            var dbContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var controller = new InvoicesApiController(
                dbContext.Object,
                new Mock<IInvoiceService>().Object,
                new Mock<IStorageService>().Object);
            var request = new GetInvoicesByExternalIdsRequest
            {
                ExternalIdentifier = "external-system",
                ExternalIds = Enumerable.Range(1, 1001).Select(i => i.ToString()).ToArray()
            };

            var result = await controller.GetByExternal(request);

            var badRequest = result.Result.ShouldBeOfType<BadRequestObjectResult>();
            badRequest.Value.ShouldBe("A maximum of 1000 externalIds can be requested per batch.");
        }
    }
}
