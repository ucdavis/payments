using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Payments.Core.Domain
{
    public class Invoice
    {
        public Invoice()
        {
            Items = new List<LineItem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public User Creator { get; set; }

        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }

        [EmailAddress]
        public string CustomerEmail { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }

        public FinancialAccount Account { get; set; }

        public PaymentEvent Payment { get; set; }

        [Required]
        public Team Team { get; set; }

        public List<LineItem> Items { get; set; }

        protected internal static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Invoice>()
                .HasOne(i => i.Creator)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Invoice>()
                .HasOne(i => i.Team)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
