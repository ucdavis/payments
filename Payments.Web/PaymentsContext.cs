using Microsoft.EntityFrameworkCore;
using Payments.Core.Models;

namespace Payments.Web
{
    public class PaymentsContext : DbContext
    {
        public PaymentsContext(DbContextOptions options) : base(options)
        {
            
        }

        public DbSet<Invoice> Invoices { get; set; }
    }
}
