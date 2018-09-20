using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Domain;

namespace Payments.Core.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        [Obsolete("Just use for tests")]
        public ApplicationDbContext() { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public virtual DbSet<Team> Teams { get; set; }

        public virtual DbSet<History> History { get; set; }

        public virtual DbSet<Invoice> Invoices { get; set; }

        public virtual DbSet<LineItem> LineItems { get; set; }

        public virtual DbSet<PaymentEvent> PaymentEvents { get; set; }

        public virtual DbSet<TeamRole> TeamRoles { get; set; }

        public virtual DbSet<TeamPermission> TeamPermissions { get; set; }

        public virtual DbSet<FinancialAccount> FinancialAccounts { get; set; }

        public virtual DbSet<LogMessage> Logs { get; set; }

        public virtual DbSet<MoneyMovementJobRecord> MoneyMovementJobRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            Invoice.OnModelCreating(builder);
            LogMessage.OnModelCreating(builder);
        }
    }
}
