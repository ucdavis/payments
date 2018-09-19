using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Payments.Core.Domain
{
    public class TeamPermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Team")]
        public Team Team { get; set; }
        public int TeamId { get; set; }

        [Required]
        [Display(Name = "User")]
        public User User { get; set; }
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Role")]
        public TeamRole Role { get; set; }
        public int RoleId { get; set; }

    }
}
