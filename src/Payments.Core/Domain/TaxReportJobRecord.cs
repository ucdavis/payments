using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Payments.Core.Domain
{
    public class TaxReportJobRecord
    {
        public string Id { get; set; }

        public string Name { get; set; }

        [Display(Name = "Ran On")]
        public DateTime RanOn { get; set; }

        public string Status { get; set; }

        public IList<LogMessage> Logs { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaxReportJobRecord>()
                .HasMany(r => r.Logs)
                .WithOne()
                .HasForeignKey(l => l.JobId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
