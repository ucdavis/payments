using System.ComponentModel.DataAnnotations;

namespace Payments.Mvc.Models.SystemViewModels
{
    public class CreateSystemAdminViewModel
    {
        [Display(Name = "Email or Kerberos")]
        [Required]
        public string UserLookup { get; set; }

    }
}
