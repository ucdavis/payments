using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Payments.Core;
using Payments.Controllers;
using Payments.Core.Models;
using Payments.Mappings;
using Payments.Models;
using Payments.Tests.Helpers;
using Xunit;

namespace Payments.Tests.ControllerTests
{
    public class InvoiceControllerTests
    {

        [Fact]
        public void TestIndexReturnViewWithData()
        {
            var data = new List<Invoice>
            {
                CreateValidEntities.Invoice(1),
                CreateValidEntities.Invoice(2),
                CreateValidEntities.Invoice(3),
            }.AsQueryable();
            
            var mockContext = new Mock<PaymentsContext>();
            mockContext.Setup(m => m.Invoices).Returns(data.MockDbSet().Object);



            var mapper = new Mock<IMapper>(); 
            var controller = new InvoiceController(mockContext.Object, mapper.Object);

            var result = controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Invoice[]>(viewResult.Model);
            Assert.Equal(3, model.Count());
            Assert.Equal("Title2", model[1].Title);
        }

        [Fact]
        public void TestCreateGetReturnView()
        {
            var mockContext = new Mock<PaymentsContext>();
            var mapper = new Mock<IMapper>(); 
            var controller = new InvoiceController(mockContext.Object, mapper.Object);

            var result = controller.Create();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task TestCreateAddsInvoiceAndRedirectsToIndex()
        {
            var data = new InvoiceEditViewModel() {Title = "Jason", TotalAmount = 14.2m};
            var mockSet = new Mock<DbSet<Invoice>>();


            var mockContext = new Mock<PaymentsContext>();
            mockContext.Setup(a => a.Invoices).Returns(mockSet.Object);
           
            var mapper = new Mock<IMapper>();
            mapper.Setup(a => a.Map<Invoice>(It.IsAny<InvoiceEditViewModel>())).Returns(CreateValidEntities.Invoice(5));
            
            
            var controller = new InvoiceController(mockContext.Object, mapper.Object);

            var result = await controller.Create(data);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(null, redirectResult.ControllerName);

            mockSet.Verify(a => a.Add(It.IsAny<Invoice>()), Times.Once());
            mockContext.Verify(a => a.SaveChangesAsync(new CancellationToken()), Times.Once);
        }

    }
}
