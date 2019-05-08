using System;
using Payments.Core.Domain;

namespace Payments.Emails.Models
{
    public class NewTeamMemberViewModel
    {
        public Team Team { get; set; }

        public User User { get; set; }

        public TeamRole Role { get; set; }
    }
}
