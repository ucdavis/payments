using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Payments.Core.Domain
{
    public class Team
    {
        [Key]
        public int Id { get; set; }

        [StringLength(128)]
        [Display(Name = "Team Name")]
        public string Name { get; set; }

        public Accounts DefaultAccount { get; set; }
        public List<Accounts> Accounts { get; set; }
    }
}
