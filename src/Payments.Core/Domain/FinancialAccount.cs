using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Payments.Core.Domain
{
    public class FinancialAccount
    {
        [Key]
        public int Id { get; set; }

        [StringLength(128)]
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [StringLength(1)]
        [Required]
        public string Chart { get; set; }

        [StringLength(7)]
        [Required]
        public string Account { get; set; }

        [StringLength(5)]
        public string SubAccount { get; set; }
    }
}
