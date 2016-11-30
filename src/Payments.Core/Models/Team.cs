using System.Collections.Generic;

namespace Payments.Core.Models
{
    public class Team : DomainObject
    {
        public string Name { get; set; }
        public bool Active { get; set; }
        public ICollection<Account> Accounts { get; set; }
    }
}