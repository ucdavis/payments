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
        public virtual DbSet<LineItem> LineItems { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Team> Teams { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Account> Accounts { get; set; }
    }
}
