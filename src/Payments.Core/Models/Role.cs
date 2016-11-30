namespace Payments.Core.Models
{
    public class Role : DomainObject
    {
        public User User { get; set; }
        public Team Team { get; set; }

        public TeamRoles TeamRole  { get; set; }
    }
}