using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Mvc.Models.TeamViewModels
{
    public class EditTeamViewModel : BaseTeamViewModel
    {
        [Display(Name = "Active?")]
        public bool IsActive { get; set; }

        [Display(Name = "Use Custom Account Validation")]
        public bool UseCustomAccountValidation { get; set; }

        [StringLength(6)]
        [RegularExpression("^[0-9A-Z]{6}$", ErrorMessage = "Select a six-character natural account from Aggie Enterprise.")]
        [Display(Name = "Recharge Debit Natural Account")]
        public string RechargeDebitNaturalAccount { get; set; }
    }
}
