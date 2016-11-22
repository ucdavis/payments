using Microsoft.EntityFrameworkCore;
using Payments.Core.Models;

namespace Payments.Core
{
    public class PaymentsContext : DbContext
    {
        public PaymentsContext(DbContextOptions<PaymentsContext> options) : base(options)
        {
            
        }

        public DbSet<Invoice> Invoices { get; set; }
    }
}
