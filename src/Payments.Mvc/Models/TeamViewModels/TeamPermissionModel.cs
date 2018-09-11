using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Payments.Core.Domain;

namespace Payments.Mvc.Models.TeamViewModels
{
    public class TeamPermissionModel
    {
        public Team Team { get; set; }

        public TeamPermission TeamPermission { get; set; }

        public SelectList Roles { get; set; }

        [Display(Name = "Email or Kerb")]
        [Required]
        public string UserLookup { get; set; }

        public int SelectedRole { get; set; }
    }
}
