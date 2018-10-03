using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Payments.Core.Domain
{
    public class Invoice
    {
        public Invoice()
        {
            Attachments = new List<InvoiceAttachment>();
            Items = new List<LineItem>();
            History = new List<History>();

            CreatedAt = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }

        public string LinkId { get; set; }

        [DisplayName("Customer Name")]
        public string CustomerName { get; set; }

        [DisplayName("Customer Address")]
        public string CustomerAddress { get; set; }

        [EmailAddress]
        [DisplayName("Customer Email")]
        public string CustomerEmail { get; set; }

        [DataType(DataType.MultilineText)]
        public string Memo { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Discount { get; set; }

        [DisplayFormat(DataFormatString = "{0:P}")]
        [DisplayName("Tax Percentage")]
        public decimal TaxPercent { get; set; }

        public DateTime? DueDate { get; set; }

        public string Status { get; set; }

        public FinancialAccount Account { get; set; }

        public PaymentEvent Payment { get; set; }

        [JsonIgnore]
        public IList<InvoiceAttachment> Attachments { get; set; }

        [JsonIgnore]
        [Required]
        public Team Team { get; set; }

        [NotMapped]
        public string TeamName => Team?.Name;

        public IList<LineItem> Items { get; set; }

        public bool Sent { get; set; }

        [DisplayName("Sent At")]
        public DateTime? SentAt { get; set; }

        [DisplayName("Created On")]
        public DateTime CreatedAt { get; set; }

        [JsonIgnore]
        public IList<History> History { get; set; }

        // ----------------------
        // Calculated Values
        // ----------------------
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Subtotal { get; private set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        [DisplayName("Tax")]
        public decimal TaxAmount { get; private set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Total { get; private set; }

        public void UpdateCalculatedValues()
        {
            Subtotal = Items.Sum(i => i.Total);
            TaxAmount = (Subtotal - Discount) * TaxPercent;
            Total = Subtotal - Discount + TaxAmount;
        }

        protected internal static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Invoice>()
                .HasOne(i => i.Team)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Invoice>()
                .Property(i => i.TaxPercent)
                .HasColumnType("decimal(18,5)");

            builder.Entity<Invoice>()
                .Property(i => i.Subtotal)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Invoice>()
                .Property(i => i.Discount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Invoice>()
                .Property(i => i.TaxAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Invoice>()
                .Property(i => i.Total)
                .HasColumnType("decimal(18,2)");
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

        public static class StatusCodes
        {
            public const string Draft = "Draft";
            public const string Sent = "Sent";
            public const string Paid = "Paid";
            public const string Processing = "Processing";
            public const string Completed = "Completed";
            public const string Cancelled = "Cancelled";

            public static string[] GetAllCodes()
            {
                return new[]
                {
                    Draft,
                    Sent,
                    Paid,
                    Processing,
                    Completed,
                    Cancelled,
                };
            }

            public static string GetBadgeClass(string status)
            {
                switch (status)
                {
                    case Draft:
                        return "badge-warning";

                    case Processing:
                    case Sent:
                        return "badge-info";

                    case Completed:
                    case Paid:
                        return "badge-success";

                    case Cancelled:
                        return "badge-danger";

                    default:
                        return "badge-secondary";
                }
            }
        }
    }
}
