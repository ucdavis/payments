using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Mvc.Models.TeamViewModels
{
    public class BaseTeamViewModel
    {
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

        [StringLength(128)]
        [Display(Name = "Contact Name")]
        [Required]
        public string ContactName { get; set; }

        [EmailAddress]
        [StringLength(128)]
        [Display(Name = "Contact Email")]
        [Required]
        public string ContactEmail { get; set; }

        [Phone]
        [StringLength(40)]
        [Display(Name = "Contact Phone Number")]
        [Required]
        public string ContactPhoneNumber { get; set; }
    }
}
