using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Payments.Core;
using Payments.Core.Models;
using Payments.Tests.Helpers;
using Shouldly;
using Xunit;

namespace Payments.Tests
{
    public class AsyncQueryTests
    {
        [Fact]
        public void test()
        {
            var data = new List<Invoice>
            {
                CreateValidEntities.Invoice(1),
                CreateValidEntities.Invoice(2),
                CreateValidEntities.Invoice(3),
            }.AsQueryable();
            var mockSet = data.MockDbSet();
            var mockContext = new Mock<PaymentsContext>();
            mockContext.Setup(m => m.Invoices).Returns(mockSet.Object);

            var invoice = mockContext.Object.Invoices.FirstOrDefault(i => i.Id == 2);

            invoice.ShouldNotBe(null);
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
            var mockSet = new Mock<DbSet<Invoice>>();
            mockSet.As<IAsyncEnumerable<Invoice>>()
                .Setup(m => m.GetEnumerator())
                .Returns(new TestAsyncEnumerator<Invoice>(data.GetEnumerator()));


            mockSet.As<IQueryable<Invoice>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<Invoice>(data.Provider));

            mockSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<PaymentsContext>();
            mockContext.Setup(m => m.Invoices).Returns(mockSet.Object);

            var query = from b in mockContext.Object.Invoices
                        orderby b.Title
                        select b;

            var invoice = await query.ToListAsync();

            //var invoice = await mockContext.Object.Invoices.FirstOrDefaultAsync(i => i.Id == 2);

            invoice.ShouldNotBe(null);
        }
    }
}
