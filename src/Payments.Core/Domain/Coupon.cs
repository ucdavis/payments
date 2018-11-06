using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Payments.Core.Domain
{
    public class Coupon
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public decimal? DiscountPercent { get; set; }

        public decimal? DiscountAmount { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [JsonIgnore]
        [Required]
        public Team Team { get; set; }

        public int TeamId { get; set; }

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
