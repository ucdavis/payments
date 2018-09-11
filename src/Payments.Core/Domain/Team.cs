using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace Payments.Core.Domain
{
    public class Team
    {
        public Team()
        {
            Accounts = new List<FinancialAccount>();
            Permissions = new List<TeamPermission>();
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
        [RegularExpression(SlugRegex,
            ErrorMessage = "Slug may only contain lowercase alphanumeric characters or single hyphens, and cannot begin or end with a hyphen")]
        public string Slug { get; set; }

        public const string SlugRegex = "^([a-z0-9]+[a-z0-9\\-]?)+[a-z0-9]$";

        [StringLength(128)]
        [Display(Name = "Contact Name")]
        public string ContactName { get; set; }

        [EmailAddress]
        [StringLength(128)]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; }

        [Phone]
        [StringLength(40)]
        [Display(Name = "Contact Phone Number")]
        public string ContactPhoneNumber { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public IList<FinancialAccount> Accounts { get; set; }

        [JsonIgnore]
        public IList<TeamPermission> Permissions { get; set; }

        [NotMapped]
        public FinancialAccount DefaultAccount {
            get {
                return Accounts.FirstOrDefault(a => a.IsDefault);
            }
        }

        public TeamPermission AddPermission(User user, TeamRole role)
        {
            var permission = new TeamPermission()
            {
                Team = this,
                User = user,
                Role = role,
            };

            Permissions.Add(permission);

            return permission;
        }
    }
}
