using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Core.Domain
{
    public class RechargeAccount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [JsonIgnore]
        public Invoice Invoice { get; set; }

        public CreditDebit Direction { get; set; } = CreditDebit.Credit;

        [Required]
        [StringLength(128)]
        public string FinancialSegmentString { get; set; }

        [Required]
        [Range(0.01, 1_000_000)]
        public decimal Amount { get; set; }

        public decimal Percentage { get; set; }

        [StringLength(20)]
        public string EnteredByKerb { get; set; }
        [StringLength(128)]
        public string EnteredByName { get; set; } //Probably Name (email) format

        [StringLength(20)]
        public string ApprovedByKerb { get; set; }
        [StringLength(128)]
        public string ApprovedByName { get; set; }

        [StringLength(400)]
        public string Notes { get; set; }



        public enum CreditDebit
        {
            /// <summary>
            /// Add money to an account
            /// </summary>
            Credit,

            /// <summary>
            /// Remove money from an account
            /// </summary>
            Debit
        }

        protected internal static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<RechargeAccount>().Property(a => a.Amount).HasColumnType("decimal(18,2)");
            builder.Entity<RechargeAccount>().Property(a => a.Percentage).HasColumnType("decimal(5,2)");
            builder.Entity<RechargeAccount>().HasOne(a => a.Invoice).WithMany(i => i.RechargeAccounts).HasForeignKey(a => a.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<RechargeAccount>().HasIndex(a => a.ApprovedByKerb);
            builder.Entity<RechargeAccount>().HasIndex(a => a.EnteredByKerb);
        }
    }
}
