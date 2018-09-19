using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Payments.Core.Domain;

namespace Payments.Mvc.Models.TeamViewModels
{
    public class CreateTeamPermissionViewModel
    {
        [DisplayName("Team")]
        public string TeamName { get; set; }

        public SelectList Roles { get; set; }

        [Display(Name = "Email or Kerberos")]
        [Required]
        public string UserLookup { get; set; }

        [DisplayName("Role")]
        public int SelectedRole { get; set; }
    }
}
