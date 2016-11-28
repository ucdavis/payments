using System;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Models;

namespace Payments.Core
{
    public class PaymentsContext : DbContext
    {
        public PaymentsContext(DbContextOptions<PaymentsContext> options) : base(options)
        {
            
        }

        [Obsolete("Just use for tests")]
        public PaymentsContext()
        {

        }

        public virtual DbSet<Invoice> Invoices { get; set; }
    }
}
