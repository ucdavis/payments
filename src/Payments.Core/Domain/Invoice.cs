﻿using System;
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

        public Dictionary<string, string> GetPaymentDictionary()
        {
            var dictionary = new Dictionary<string, string>
            {
                {"transaction_type"       , "sale"},
                {"reference_number"       , Id.ToString()},
                {"amount"                 , Total.ToString("F2")},
                {"currency"               , "USD"},
                {"transaction_uuid"       , Guid.NewGuid().ToString()},
                {"signed_date_time"       , DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")},
                {"unsigned_field_names"   , string.Empty},
                {"locale"                 , "en"},
                {"bill_to_email"          , CustomerEmail},
                {"bill_to_forename"       , CustomerName},
                {"bill_to_address_country", "US"},
                {"bill_to_address_state"  , "CA"}
            };

            dictionary.Add("line_item_count", Items.Count.ToString("D"));
            for (var i = 0; i < Items.Count; i++)
            {
                dictionary.Add($"item_{i}_name", Items[i].Description);
                dictionary.Add($"item_{i}_quantity", Items[i].Quantity.ToString());
                dictionary.Add($"item_{i}_unit_price", Items[i].Amount.ToString("F2"));
            }

            return dictionary;
        }
    }
}
