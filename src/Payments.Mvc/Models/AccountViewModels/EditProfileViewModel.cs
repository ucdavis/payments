using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payments.Mvc.Models.AccountViewModels
{
    public class EditProfileViewModel
    {
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [StringLength(256)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(256)]
        [EmailAddress]
        public string Email { get; set; }
    }
}
