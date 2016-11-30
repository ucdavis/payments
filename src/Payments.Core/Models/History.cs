using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Core.Models
{
    public class History : DomainObject
    {
        public DateTime ActedDate { get; set; }
        public string ActorName { get; set; }
        public User Actor { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; }

        public InvoiceStatus InvoiceStatus { get; set; }

        [Required]
        public Invoice Invoice { get; set; }
    }
}