using System;
using System.ComponentModel.DataAnnotations;
using Payments.Core.Domain;

namespace Payments.Mvc.Models.FinancialModels
{
    public class FinancialAccountModel 
    {
        [Key]
        public int Id { get; set; }

        [StringLength(128)]
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [StringLength(1)]
        public string Chart { get; set; }

        [StringLength(7)]
        public string Account { get; set; }

        [StringLength(5)]
        [DisplayFormat(NullDisplayText = "-----")]
        public string SubAccount { get; set; }

        [StringLength(9)]
        [DisplayFormat(NullDisplayText = "---------")]
        public string Project { get; set; }

        [Display(Name="Default Account")]
        public bool IsDefault { get; set; }

        [Display(Name="Active")]
        public bool IsActive { get; set; } = true;

        public Team Team { get; set; }

        public KfsAccount KfsAccount { get; set; }

        [StringLength(128)]
        [Display(Name = "AE Financial Segment String (COA)")]
        public string FinancialSegmentString { get; set; }
    }
}
