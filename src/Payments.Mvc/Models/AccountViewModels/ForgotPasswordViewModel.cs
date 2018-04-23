using System.ComponentModel.DataAnnotations;

namespace Payments.Mvc.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
