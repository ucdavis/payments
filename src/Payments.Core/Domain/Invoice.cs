using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Payments.Core.Domain
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        public User Creator { get; set; }
        public string CreatorId { get; set; }

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
        public int AccountId { get; set; }
        public PaymentEvent Payment { get; set; }
        public string PaymentId { get; set; }

        [Required]
        public Team Team { get; set; }
        public int TeamId { get; set; }
    }
}
