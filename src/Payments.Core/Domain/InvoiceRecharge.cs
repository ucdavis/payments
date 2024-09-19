using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Payments.Core.Domain
{
    public class InvoiceRecharge
    {
        [Key]
        public int Id { get; set; }
        public string FinancialSegmenmtString { get; set; }
        public decimal Percentage { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public bool IsActive { get; set; } = true;

        public int InvoiceId { get; set; }
        [Required]
        [JsonIgnore]
        public Invoice Invoice { get; set; }

        protected internal static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<InvoiceRecharge>()
                .Property(i => i.Percentage)
                .HasColumnType("decimal(18,5)");

        }

    }
}
