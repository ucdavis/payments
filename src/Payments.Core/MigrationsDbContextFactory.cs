using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Payments.Core
{
    public class MigrationsDbContextFactory : IDbContextFactory<PaymentsContext>
    {
        public PaymentsContext Create(DbContextFactoryOptions options)
        {
            var builder = new DbContextOptionsBuilder<PaymentsContext>();

            var connection = @"Server=.\sqlexpress;Database=Payments;Trusted_Connection=True;";
            builder.UseSqlServer(connection);

            return new PaymentsContext(builder.Options);
        }
    }
}
