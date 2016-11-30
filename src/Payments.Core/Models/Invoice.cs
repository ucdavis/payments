using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Core.Models
{
    public class Invoice : DomainObject
    {
        [Required]
        [StringLength(255)]
        public string ClientName { get; set; }
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string ClientEmail { get; set; }

        // Billing address
        // Shipping address

        // Title of invoice so creator can remember what it is for.  Optional
        [StringLength(128)]
        public string Title { get; set; }

        [Required]
        [Range(0.01, int.MaxValue, ErrorMessage = "Must be at least $0.01")]
        public decimal TotalAmount { get; set; }

        [Required]
        public InvoiceStatus Status { get; set; }
    }
}
