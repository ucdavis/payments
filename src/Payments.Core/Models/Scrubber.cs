using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Core.Models
{
    public class Scrubber : DomainObject
    {
        public Scrubber()
        {
            Created = DateTime.UtcNow;
        }

        [Required]
        [StringLength(50)]
        public string OriginCode { get; set; }

        [Required]
        [StringLength(50)]
        public string DocumentNumber { get; set; }

        [StringLength(50)]
        public string FileName { get; set; }

        public DateTime Created { get; set; }

        [Required]
        public Invoice Invoice { get; set; }
    }
}