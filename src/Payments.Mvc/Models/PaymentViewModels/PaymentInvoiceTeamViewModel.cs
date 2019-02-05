using System.ComponentModel.DataAnnotations;
using Payments.Core.Domain;

namespace Payments.Mvc.Models.PaymentViewModels
{
    public class PaymentInvoiceTeamViewModel
    {
        public PaymentInvoiceTeamViewModel() { }

        public PaymentInvoiceTeamViewModel(Team team)
        {
            Name               = team.Name;
            ContactName        = team.ContactName;
            ContactEmail       = team.ContactEmail;
            ContactPhoneNumber = team.ContactPhoneNumber;
        }

        [StringLength(128)]
        [Display(Name = "Team Name")]
        [Required]
        public string Name { get; set; }

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
    }
}
