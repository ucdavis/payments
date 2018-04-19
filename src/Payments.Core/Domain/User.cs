using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Payments.Core.Domain
{
    public class User : IdentityUser
    {
        // IAM (campus identity) ID
        [Key]
        public string Id { get; set; }

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

        public List<TeamPermission> TeamPermissions { get; set; }
    }
}
