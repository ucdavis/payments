using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Models
{
    public class InvoiceEditViewModel
    {
        [Required]
        public string Title { get; set; }
        [Range(0.01, int.MaxValue, ErrorMessage = "Must be great than $0.00")]
        public decimal TotalAmount { get; set; }
    }
}
