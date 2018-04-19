using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Payments.Core.Domain
{
    public class LineItem
    {
        [Key]
        public int Id { get; set; } 
        
        public string Description { get; set; }

        [Range(1, Int32.MaxValue)]
        public int Quantity { get; set; }

        public decimal Amount { get; set; }
        
        public decimal Total { get; set; }

        [Required]
        public Invoice Invoice { get; set; }
        public int InvoiceId { get; set; }
    }
}
