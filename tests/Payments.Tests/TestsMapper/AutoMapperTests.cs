using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Payments.Core.Models;
using Payments.Mappings;
using Payments.Models;
using Payments.Tests.Helpers;
using Shouldly;
using Xunit;

namespace Payments.Tests.TestsMapper
{
    public class AutoMapperTests
    {

        [Fact]
        public void TestInvoiceConfigIsValid()
        {
            //Arrange
            Mapper.Initialize(a => a.AddProfile(typeof(InvoiceMappingProfile)));

            //Assert
            Mapper.Configuration.AssertConfigurationIsValid();
        }
        [Fact]
        public void TestInvoiceEditViewModelMapsToInvoiceForTitle()
        {
            //Arrange
            Mapper.Initialize(a => a.AddProfile(typeof(InvoiceMappingProfile)));
            //Mapper.Configuration.AssertConfigurationIsValid();
            var source = CreateValidEntities.InvoiceEditViewModel(3, true);

            //Act
            var result = Mapper.Map<Invoice>(source);

            //Assert
            result.Title.ShouldBe("Title3");
        }

        [Fact]
        public void TestInvoiceEditViewModelMapsToInvoiceForTotalAmount()
        {
            //Arrange
            Mapper.Initialize(a => a.AddProfile(typeof(InvoiceMappingProfile)));
            //Mapper.Configuration.AssertConfigurationIsValid();
            var source = CreateValidEntities.InvoiceEditViewModel(3, true);

            //Act
            var result = Mapper.Map<Invoice>(source);

            //Assert
            result.TotalAmount.ShouldBe(3.00m);
        }

        [Fact]
        public void TestInvoiceMapsToInvoiceEditViewModelForTitle()
        {
            //Arrange
            Mapper.Initialize(a => a.AddProfile(typeof(InvoiceMappingProfile)));
            //Mapper.Configuration.AssertConfigurationIsValid();
            var source = CreateValidEntities.Invoice(3, true);

            //Act
            var result = Mapper.Map<InvoiceEditViewModel>(source);

            //Assert
            result.TotalAmount.ShouldBe(3.00m);
        }

        [Fact]
        public void TestInvoiceMapsToInvoiceEditViewModelForTotalAmount()
        {
            //Arrange
            Mapper.Initialize(a => a.AddProfile(typeof(InvoiceMappingProfile)));
            //Mapper.Configuration.AssertConfigurationIsValid();
            var source = CreateValidEntities.Invoice(3, true);

            //Act
            var result = Mapper.Map<InvoiceEditViewModel>(source);

            //Assert
            result.TotalAmount.ShouldBe(3.00m);
        }
    }
}
