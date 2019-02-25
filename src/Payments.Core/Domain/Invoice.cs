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

            DraftCount = 0;
            CreatedAt = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }

        public string LinkId { get; set; }

        public int DraftCount { get; set; }

        public string GetFormattedId()
        {
            return $"{Id:D3}-{DraftCount:D3}";
        }

        [DisplayName("Customer Name")]
        public string CustomerName { get; set; }

        [DisplayName("Customer Address")]
        public string CustomerAddress { get; set; }

        [EmailAddress]
        [DisplayName("Customer Email")]
        public string CustomerEmail { get; set; }

        [DataType(DataType.MultilineText)]
        public string Memo { get; set; }

        [DisplayFormat(DataFormatString = "{0:P}")]
        [DisplayName("Tax Percentage")]
        public decimal TaxPercent { get; set; }

        public DateTime? DueDate { get; set; }

        public string Status { get; set; }

        public Coupon Coupon { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal ManualDiscount { get; set; }

        public FinancialAccount Account { get; set; }

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

        public bool Paid { get; set; }

        [DisplayName("Paid On")]
        public DateTime? PaidAt { get; set; }

        public string PaymentType { get; set; }

        public string PaymentProcessorId { get; set; }

        [DisplayName("Created On")]
        public DateTime CreatedAt { get; set; }

        public bool Deleted { get; set; }

        [DisplayName("Deleted On")]
        public DateTime? DeletedAt { get; set; }

        [JsonIgnore]
        public IList<History> History { get; set; }

        [JsonIgnore]
        public IList<PaymentEvent> PaymentEvents { get; set; }

        // ----------------------
        // Calculated Values
        // ----------------------
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal CalculatedSubtotal { get; private set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal CalculatedDiscount { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        [DisplayName("Taxable Amount")]
        public decimal CalculatedTaxableAmount { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        [DisplayName("Tax")]
        public decimal CalculatedTaxAmount { get; private set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal CalculatedTotal { get; private set; }

        public decimal GetDiscountAmount()
        {
            // return saved value if the invoice is already paid
            if (Paid)
            {
                return ManualDiscount;
            }

            // check for valid coupon on unpaid invoices
            if (Coupon == null)
            {
                return ManualDiscount;
            }

            // check for expired coupon on unpaid invoices
            if (Coupon.ExpiresAt != null && Coupon.ExpiresAt.Value < DateTime.UtcNow)
            {
                return 0;
            }

            if (Coupon.DiscountAmount.HasValue)
            {
                return Coupon.DiscountAmount ?? 0;
            }

            var subTotal = Items.Sum(i => i.Total);
            if (Coupon.DiscountPercent.HasValue)
            {
                return Coupon.DiscountPercent * subTotal ?? 0;
            }

            return 0;
        }

        public decimal GetTaxableTotal()
        {
            var subtotal = Items.Sum(i => i.Total);
            var discount = GetDiscountAmount();

            // remove tax exempt items, apply proportional part of discount, then calculate tax
            var taxableAmount = Items.Where(i => !i.TaxExempt).Sum(i => i.Total);
            var taxableDiscount = discount * (taxableAmount / subtotal);
            return (taxableAmount - taxableDiscount);
        }

        public decimal GetTaxAmount()
        {
            var taxableTotal = GetTaxableTotal();
            return taxableTotal * TaxPercent;
        }

        public void UpdateCalculatedValues()
        {
            CalculatedSubtotal = Items.Sum(i => i.Total);

            CalculatedDiscount = GetDiscountAmount();

            CalculatedTaxableAmount = GetTaxableTotal();

            CalculatedTaxAmount = GetTaxAmount();

            CalculatedTotal = CalculatedSubtotal - CalculatedDiscount + CalculatedTaxAmount;
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
                .Property(i => i.CalculatedSubtotal)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Invoice>()
                .Property(i => i.CalculatedDiscount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Invoice>()
                .Property(i => i.CalculatedTaxAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Invoice>()
                .Property(i => i.CalculatedTotal)
                .HasColumnType("decimal(18,2)");
        }

        public Dictionary<string, string> GetPaymentDictionary()
        {
            var dictionary = new Dictionary<string, string>
            {
                {"transaction_type"       , "sale"},
                {"reference_number"       , Id.ToString()},             // use the actual id so we can find it easily
                {"amount"                 , CalculatedTotal.ToString("F2")},
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
                //dictionary.Add($"item_{i}_quantity", Items[i].Quantity.ToString());
                dictionary.Add($"item_{i}_unit_price", Items[i].Total.ToString("F2"));
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
            public const string Deleted = "Deleted";

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
