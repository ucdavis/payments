using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Moq;
using Payments.Core;
using Payments.Controllers;
using Payments.Core.Models;
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
            
            var mockSet = MockInvoice(data);
            var mockContext = new Mock<PaymentsContext>();
            mockContext.Setup(m => m.Invoices).Returns(mockSet.Object);



            var mapper = new Mock<IMapper>(); //Probably don't want to mock this. Maybe assign it directly?
            var controller = new InvoiceController(mockContext.Object, mapper.Object);

            var result = controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Invoice[]>(viewResult.Model);
            Assert.Equal(3, model.Count());
            Assert.Equal("Title2", model[1].Title);
        }

        private static Mock<DbSet<Invoice>> MockInvoice(IQueryable<Invoice> data)
        {
            var mockSet = new Mock<DbSet<Invoice>>();

            mockSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }
    }
}
