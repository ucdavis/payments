using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Payments.Core.Domain
{
    public class LineItem
    {
        [Key]
        public int Id { get; set; } 
        
        public string Description { get; set; }

        [Range(0, 1_000_000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        [Range(0, 1_000_000)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Range(0, 1_000_000)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public bool TaxExempt { get; set; }
    }
}
