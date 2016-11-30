using System.ComponentModel.DataAnnotations;

namespace Payments.Core.Models
{
    public class LineItem : DomainObject
    {
        [Required]
        public Invoice Invoice { get; set; }

        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Note { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0.01, int.MaxValue, ErrorMessage = "Must be at least $0.01")]
        public decimal Amount { get; set; }

        [Range(0.00, 100.0, ErrorMessage = "Cannot be negative or greater than 100%")]
        public decimal TaxPercentage { get; set; }

        public bool RequiresShipping { get; set; }
    }
}