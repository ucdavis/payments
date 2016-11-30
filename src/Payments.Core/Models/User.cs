using System.Collections.Generic;

namespace Payments.Core.Models
{
    public class User : DomainObject<string>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool Active { get; set; }

        public ICollection<Team> Teams { get; set; }
    }
}