using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Payments.Core.Domain
{
    public class Team
    {
        public Team()
        {
            Accounts = new List<FinancialAccount>();
        }

        [Key]
        public int Id { get; set; }

        [StringLength(128)]
        [Display(Name = "Team Name")]
        public string Name { get; set; }

        // public FinancialAccount DefaultAccount { get; set; }
        // public int DefaultAccountId { get; set; }
        public List<FinancialAccount> Accounts { get; set; }
    }
}
