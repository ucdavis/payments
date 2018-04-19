using Microsoft.EntityFrameworkCore;
using Payments.Core.Domain;

namespace Payments.Core.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public virtual DbSet<Team> Teams { get; set; }
        public virtual DbSet<History> History { get; set; }
        public virtual DbSet<Invoice> Invoice { get; set; }
        public virtual DbSet<LineItem> LineItems { get; set; }
        public virtual DbSet<PaymentEvent> PaymentEvents { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<TeamPermission> TeamPermissions { get; set; }
        public virtual DbSet<User> Users { get; set; }

    }
}
