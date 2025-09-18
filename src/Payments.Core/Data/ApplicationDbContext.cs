using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Domain;

namespace Payments.Core.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public virtual DbSet<Team> Teams { get; set; }

        public virtual DbSet<History> History { get; set; }

        public virtual DbSet<Invoice> Invoices { get; set; }

        public virtual DbSet<InvoiceLink> InvoiceLinks { get; set; }

        public virtual DbSet<LineItem> LineItems { get; set; }

        public virtual DbSet<Coupon> Coupons { get; set; }

        public virtual DbSet<PaymentEvent> PaymentEvents { get; set; }

        public virtual DbSet<InvoiceAttachment> InvoiceAttachments { get; set; }

        public virtual DbSet<TeamRole> TeamRoles { get; set; }

        public virtual DbSet<TeamPermission> TeamPermissions { get; set; }

        public virtual DbSet<FinancialAccount> FinancialAccounts { get; set; }

        public virtual DbSet<WebHook> WebHooks { get; set; }

        public virtual DbSet<LogMessage> Logs { get; set; }

        public virtual DbSet<MoneyMovementJobRecord> MoneyMovementJobRecords { get; set; }

        public virtual DbSet<TaxReportJobRecord> TaxReportJobRecords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

#if DEBUG
            optionsBuilder.EnableSensitiveDataLogging();
#endif
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            Coupon.OnModelCreating(builder);
            Invoice.OnModelCreating(builder);
            LogMessage.OnModelCreating(builder);
            MoneyMovementJobRecord.OnModelCreating(builder);
            TaxReportJobRecord.OnModelCreating(builder);
            Team.OnModelCreating(builder);
        }
    }
}
