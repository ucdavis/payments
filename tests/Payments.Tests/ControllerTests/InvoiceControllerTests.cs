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
using Xunit;

namespace Payments.Tests.ControllerTests
{
    public class InvoiceControllerTests
    {
        //private readonly Mock<PaymentsContext> _context;
        //private readonly Mock<IMapper> _mapper;

        //public InvoiceControllerTests()
        //{

        //   _context = new Mock<PaymentsContext>();
        //   _mapper = new Mock<IMapper>();
        //}

        public class MyDbContext : DbContext
        {
            public MyDbContext()
            {
            }

            public MyDbContext(DbContextOptions options)
                : base(options)
            {
             
            }


        }

        [Fact]
        public void Test1()
        {
            var data = new List<Invoice>
            {
                new Invoice { Title = "BBB" },
                new Invoice { Title = "ZZZ" },
                new Invoice { Title = "AAA" },
            }.AsQueryable();



            //var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
            var mockSet = new Mock<DbSet<Invoice>>();

            mockSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<PaymentsContext>();
            mockContext.Setup(m => m.Invoices).Returns(mockSet.Object);



            var mapper = new Mock<IMapper>(); //Probably don't want to mock this. Maybe assign it directly?
            var controller = new InvoiceController(mockContext.Object, mapper.Object);

            var result = controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Invoice[]>(viewResult.Model);
            Assert.Equal(3, model.Count());
            
        }

    }
}
