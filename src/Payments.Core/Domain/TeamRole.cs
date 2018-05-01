using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Payments.Core.Domain
{
    public class TeamRole
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public class Codes
        {
            //public const string Viewer = "Viewer"; Don't use yet
            public const string Editor = "Editor"; //Can do everything except manage users/roles
            public const string Admin = "Admin"; //God like powers limited to the Team
        }
    }
}
