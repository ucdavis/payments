using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Payments.Core.Domain;

namespace Payments.Mvc.Models.TeamViewModels
{
    public class TeamDetailsModel
    {
        [Display(Name = "Team Name")]
        public string Name { get; set; }

        [Display(Name = "Team Slug")]
        public string Slug { get; set; }

        [Display(Name = "Contact Name")]
        public string ContactName { get; set; }

        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; }

        [Display(Name = "Contact Phone Number")]
        public string ContactPhoneNumber { get; set; }

        [Display(Name = "ApiKey")]
        public string ApiKey { get; set; }

        [Display(Name = "Active?")]
        public bool IsActive { get; set; }

        public IList<FinancialAccount> Accounts { get; set; }

        public IList<TeamPermission> Permissions { get; set; }

        public bool UserCanEdit { get; set; }

        public bool ShowCoa { get; set; }
    }
}
