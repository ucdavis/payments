using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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

        [Fact]
        public void Test1()
        {
            //var xxx = new Mock<DbContextOptions<PaymentsContext>>();
            var context = new Mock<PaymentsContext>();
            //context.Setup
            var mapper = new Mock<IMapper>();
            var controller = new InvoiceController(context.Object, mapper.Object);
        }

    }
}
