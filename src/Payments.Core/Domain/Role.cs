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
            public const string SampleRole = "SampleRole";
            public const string SampleRole2 = "SampleRole2";

        }
    }
}
