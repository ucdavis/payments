using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Payments.Core.Domain
{
    public class Coupon
    {
        public int Id { get; set; }

        [Display(Name = "Coupon Name")]
        public string Name { get; set; }

        [Display(Name = "Coupon Code")]
        public string Code { get; set; }

        [Display(Name = "Discount Percent")]
        public decimal? DiscountPercent { get; set; }

        [Display(Name = "Discount Amount")]
        public decimal? DiscountAmount { get; set; }

        [Display(Name = "Expiration Date")]
        public DateTime? ExpiresAt { get; set; } //This is stored as a Pacific Time Date ie: 2019-10-01 00:00:00.0000000

        [JsonIgnore]
        [Required]
        public Team Team { get; set; }

        public int TeamId { get; set; }

        [JsonIgnore]
        public IList<Invoice> Invoices { get; set; }

        protected internal static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Coupon>()
                .Property(i => i.DiscountPercent)
                .HasColumnType("decimal(18,5)");

            builder.Entity<Coupon>()
                .Property(i => i.DiscountAmount)
                .HasColumnType("decimal(18,2)");
        }
    }
}
