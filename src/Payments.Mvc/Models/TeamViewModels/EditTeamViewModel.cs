using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Mvc.Models.TeamViewModels
{
    public class EditTeamViewModel
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

        [Display(Name = "Active?")]
        public bool IsActive { get; set; }
    }
}
