using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Payments.Core;
using Payments.Tests.Helpers;
using Shouldly;
using Xunit;

namespace Payments.Tests.TestsDatabase
{
    public class InvoiceDatabaseTests
    {
        [Fact]
        //[AutoRollback]
        public void Test1()
        {

            var options = new DbContextOptionsBuilder<PaymentsContext>()
                .UseInMemoryDatabase(databaseName: "UnitTestDb")
                .Options;
            using (var context = new PaymentsContext(options))
            {
                context.Database.EnsureDeleted();
            }
            using (var context = new PaymentsContext(options))
            {
                context.Invoices.Add(CreateValidEntities.Invoice(2));
                context.SaveChanges();
            }

            using (var context = new PaymentsContext(options))
            {
                var count = context.Invoices.ToArray();
                count.Count().ShouldBe(1);
            }




        }

        [Fact]
        //[AutoRollback]
        public void Test2()
        {

            var options = new DbContextOptionsBuilder<PaymentsContext>()
                .UseInMemoryDatabase(databaseName: "UnitTestDb")
                .Options;
            using (var context = new PaymentsContext(options))
            {
                context.Database.EnsureDeleted();
            }
            using (var context = new PaymentsContext(options))
            {

                var xxx = context.Invoices.ToArray();
                xxx.Count().ShouldBe(0);
            }
            using (var context = new PaymentsContext(options))
            {

                context.Invoices.Add(CreateValidEntities.Invoice(2));
                context.Invoices.Add(CreateValidEntities.Invoice(3));
                context.Invoices.Add(CreateValidEntities.Invoice(4));
                context.SaveChanges();

            }
            using (var context = new PaymentsContext(options))
            {

                var count = context.Invoices.ToArray();
                count.Count().ShouldBe(3);
            }
        }
    }
}
