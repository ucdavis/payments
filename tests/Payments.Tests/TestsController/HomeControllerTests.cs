using Microsoft.AspNetCore.Mvc;
using Payments.Controllers;
using Xunit;

namespace Payments.Tests.TestsController
{
    public class HomeControllerTests
    {

        [Fact]
        public void IndexReturnsView()
        {
            //Arange
            var controller = new HomeController();

            //Act
            var result = controller.Index();

            //Asset
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void AboutReturnsView()
        {
            //Arange
            var controller = new HomeController();

            //Act
            var result = controller.About();

            //Asset
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void AboutReturnsViewDate()
        {
            //Arange
            var controller = new HomeController();

            //Act
            var result = controller.About();

            //Asset
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(1, viewResult.ViewData.Count);
            Assert.Equal("Your application description page.", viewResult.ViewData["Message"]);
        }
    }
}
