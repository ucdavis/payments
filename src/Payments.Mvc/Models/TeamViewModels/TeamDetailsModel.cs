using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Payments.Core.Domain;

namespace Payments.Mvc.Models.Teams
{
    public class TeamDetailsModel
    {
        public Team Team { get; set; }
        public List<TeamPermission> Permissions { get; set; }
    }
}
