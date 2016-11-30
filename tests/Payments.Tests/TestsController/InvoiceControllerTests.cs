using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Payments.Controllers;
using Payments.Core;
using Payments.Core.Models;
using Payments.Mappings;
using Payments.Models;
using Payments.Tests.Helpers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Payments.Tests.TestsController
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

        [Fact]
        public async Task TestCreateWithRealMapping()
        {
            var data = new InvoiceEditViewModel() { Title = "Jason", TotalAmount = 14.2m };
            var mockSet = new Mock<DbSet<Invoice>>();
            
            var mockContext = new Mock<PaymentsContext>();
            mockContext.Setup(a => a.Invoices).Returns(mockSet.Object);

            Invoice savedResult = null;
            mockSet.Setup(a => a.Add(It.IsAny<Invoice>())).Callback<Invoice>(r => savedResult = r);

            Mapper.Initialize(a => a.AddProfile(typeof(InvoiceMappingProfile)));
            Mapper.Configuration.AssertConfigurationIsValid();


            var controller = new InvoiceController(mockContext.Object, Mapper.Instance);

            var result = await controller.Create(data);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(null, redirectResult.ControllerName);

            mockSet.Verify(a => a.Add(It.IsAny<Invoice>()), Times.Once());
            mockContext.Verify(a => a.SaveChangesAsync(new CancellationToken()), Times.Once);

            savedResult.ShouldNotBe(null);
            savedResult.Title.ShouldBe("Jason");
            savedResult.TotalAmount.ShouldBe(14.2m);
            savedResult.Id.ShouldBe(0); //Well, this is what it is, it would be incremented in the DB I guess
        }

        [Fact]
        public async Task TestCreateReturnsModelWhenModelStateInvalid()
        {
            var data = new InvoiceEditViewModel() { Title = "Jason", TotalAmount = 2.00m };
            var mockSet = new Mock<DbSet<Invoice>>();


            var mockContext = new Mock<PaymentsContext>();
            mockContext.Setup(a => a.Invoices).Returns(mockSet.Object);

            var mapper = new Mock<IMapper>();
            mapper.Setup(a => a.Map<Invoice>(It.IsAny<InvoiceEditViewModel>())).Returns(CreateValidEntities.Invoice(5));


            var controller = new InvoiceController(mockContext.Object, mapper.Object);
            controller.ModelState.AddModelError("Fake", "Error");

            var result = await controller.Create(data);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(data, viewResult.Model);


            mockSet.Verify(a => a.Add(It.IsAny<Invoice>()), Times.Never);
            mockContext.Verify(a => a.SaveChangesAsync(new CancellationToken()), Times.Never);
        }


        [Fact]
        public async Task test2()
        {
            var data = new List<Invoice>
            {
                CreateValidEntities.Invoice(1),
                CreateValidEntities.Invoice(2),
                CreateValidEntities.Invoice(3),
            }.AsQueryable();

            var mockContext = new Mock<PaymentsContext>();
            mockContext.Setup(m => m.Invoices).Returns(data.MockAsyncDbSet().Object);

            var query = from b in mockContext.Object.Invoices
                        orderby b.Title
                        select b;

            var invoice = await query.ToListAsync();

            //var invoice = await mockContext.Object.Invoices.FirstOrDefaultAsync(i => i.Id == 2);

            invoice.ShouldNotBe(null);
        }

    }

    [Trait("Category", "Controller Reflection")]
    public class InvoiceReflectionTests
    {
        private readonly ITestOutputHelper output;
        public InvoiceReflectionTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        #region Reflection Tests
        protected readonly Type ControllerClass = typeof(InvoiceController);

        #region Controller Class Tests
        
        [Fact]
        public void TestControllerInheritsFromApplicationController()
        {
            #region Arrange
            var controllerClass = ControllerClass;
            #endregion Arrange

            #region Act
            controllerClass.BaseType.ShouldNotBe(null);
            var result = controllerClass.BaseType.Name;
            #endregion Act

            #region Assert
            result.ShouldBe("ApplicationController");

            #endregion Assert
        }
        [Fact]
        public void TestControllerExpectedNumberOfAttributes()
        {
            #region Arrange
            var controllerClass = ControllerClass;
            #endregion Arrange

            #region Act
            var result = controllerClass.GetCustomAttributes(true);
            #endregion Act

            #region Assert
            foreach (var o in result)
            {
                output.WriteLine(o.ToString()); //Output shows if the test fails
            }
            result.Count().ShouldBe(1);

            #endregion Assert
        }
        [Fact]
        public void TestControllerHasControllerAttribute()
        {
            #region Arrange
            var controllerClass = ControllerClass;
            #endregion Arrange

            #region Act
            var result = controllerClass.GetCustomAttributes(true).OfType<ControllerAttribute>();
            #endregion Act

            #region Assert
            result.Count().ShouldBeGreaterThan(0, "ControllerAttribute not found.");

            #endregion Assert
        }
        #endregion Controller Class Tests

        #region Controller Method Tests
        [Fact(Skip = "Tests are still being written. When done, remove this line.")]
        public void TestControllerContainsExpectedNumberOfPublicMethods()
        {
            #region Arrange
            var controllerClass = ControllerClass;
            #endregion Arrange

            #region Act
            var result = controllerClass.GetMethods().Where(a => a.DeclaringType == controllerClass);
            #endregion Act

            #region Assert
            result.Count().ShouldBe(3);

            #endregion Assert
        }
        [Fact]
        public void TestControllerMethodIndexContainsExpectedAttributes()
        {
            #region Arrange
            var controllerClass = ControllerClass;
            var controllerMethod = controllerClass.GetMethod("Index");
            #endregion Arrange

            #region Act
            //var expectedAttribute = controllerMethod.GetCustomAttributes(true).OfType<SomeAttribute>();
            var allAttributes = controllerMethod.GetCustomAttributes(true);
            #endregion Act

            #region Assert
            foreach (var o in allAttributes)
            {
                output.WriteLine(o.ToString()); //Output shows if the test fails
            }
            allAttributes.Count().ShouldBe(0, "No Attributes");
            //Assert.AreEqual(1, expectedAttribute.Count(), "AllowGiftTeamAndResearchAccess not found");
            //Assert.AreEqual(1, allAttributes.Count());
            #endregion Assert
        }
        [Fact]
        public void TestControllerMethodCreateContainsExpectedAttributes1()
        {
            #region Arrange
            var controllerClass = ControllerClass;
            var controllerMethod = controllerClass.GetMethods().Where(a => a.Name == "Create");
            var element = controllerMethod.ElementAt(0);
            #endregion Arrange

            #region Act
            var expectedAttribute = element.GetCustomAttributes(true).OfType<HttpGetAttribute>();
            var allAttributes = element.GetCustomAttributes(true);
            #endregion Act

            #region Assert
            foreach (var attribute in allAttributes)
            {
                output.WriteLine(attribute.ToString());
            }
            expectedAttribute.Count().ShouldBe(1, "HttpGetAttribute not found");
            allAttributes.Count().ShouldBe(1);
            #endregion Assert
        }

        [Fact]
        public void TestControllerMethodCreateContainsExpectedAttributes2()
        {
            #region Arrange
            var controllerClass = ControllerClass;
            var controllerMethod = controllerClass.GetMethods().Where(a => a.Name == "Create");
            var element = controllerMethod.ElementAt(1);
            #endregion Arrange

            #region Act
            var expectedAttribute = element.GetCustomAttributes(true).OfType<HttpPostAttribute>();
            var allAttributes = element.GetCustomAttributes(true);
            #endregion Act

            #region Assert
            foreach (var attribute in allAttributes)
            {
                output.WriteLine(attribute.ToString());
            }
            expectedAttribute.Count().ShouldBe(1, "HttpPostAttribute not found");
            allAttributes.Count().ShouldBe(3);
            #endregion Assert
        }

        [Fact]
        public void TestControllerMethodCreateContainsExpectedAttributes3()
        {
            #region Arrange
            var controllerClass = ControllerClass;
            var controllerMethod = controllerClass.GetMethods().Where(a => a.Name == "Create");
            var element = controllerMethod.ElementAt(1);
            #endregion Arrange

            #region Act
            var expectedAttribute = element.GetCustomAttributes(true).OfType<AsyncStateMachineAttribute>();
            var allAttributes = element.GetCustomAttributes(true);
            #endregion Act

            #region Assert
            foreach (var attribute in allAttributes)
            {
                output.WriteLine(attribute.ToString());
            }
            expectedAttribute.Count().ShouldBe(1, "AsyncStateMachineAttribute not found");
            allAttributes.Count().ShouldBe(3);
            #endregion Assert
        }

        [Fact]
        public void TestControllerMethodCreateContainsExpectedAttributes4()
        {
            #region Arrange
            var controllerClass = ControllerClass;
            var controllerMethod = controllerClass.GetMethods().Where(a => a.Name == "Create");
            var element = controllerMethod.ElementAt(1);
            #endregion Arrange

            #region Act
            var expectedAttribute = element.GetCustomAttributes(true).OfType<DebuggerStepThroughAttribute>();
            var allAttributes = element.GetCustomAttributes(true);
            #endregion Act

            #region Assert
            foreach (var attribute in allAttributes)
            {
                output.WriteLine(attribute.ToString());
            }
            expectedAttribute.Count().ShouldBe(1, "DebuggerStepThroughAttribute not found");
            allAttributes.Count().ShouldBe(3);
            #endregion Assert
        }

        #endregion Controller Method Tests

        #endregion Reflection Tests
    }
}
