using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Mvc.Models.TeamViewModels
{
    public class EditTeamViewModel : BaseTeamViewModel
    {
        [Display(Name = "Active?")]
        public bool IsActive { get; set; }
    }
}
