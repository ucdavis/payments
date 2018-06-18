using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Payments.Core.Domain
{
    public class Team
    {
        public Team()
        {
            Accounts = new List<FinancialAccount>();
        }

        [Key]
        public int Id { get; set; }

        [StringLength(128)]
        [Display(Name = "Team Name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Team Slug")]
        [Required]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "Slug must be between 3 and 40 characters")]
        [RegularExpression("^([a-z0-9]+[a-z0-9\\-]?)+[a-z0-9]$",
            ErrorMessage = "Slug may only contain lowercase alphanumeric characters or single hyphens, and cannot begin or end with a hyphen")]
        public string Slug { get; set; }

        public bool IsActive { get; set; } = true;

        public List<FinancialAccount> Accounts { get; set; }

        [NotMapped]
        public FinancialAccount DefaultAccount {
            get {
                return Accounts.FirstOrDefault(a => a.IsDefault);
            }
        }
    }
}
