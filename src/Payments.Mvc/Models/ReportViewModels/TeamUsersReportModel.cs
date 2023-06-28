using Payments.Core.Domain;
using System;
using System.Linq.Expressions;

namespace Payments.Core.Models.Report
{
    public class TeamUsersReportModel
    {
        public int Id { get; set; }
        public string TeamName { get; set; }
        public string TeamSlug { get; set; }
        public string Kerb { get; set; }
        public string UserEmail { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string RoleName { get; set; }

        public bool IsActive { get; set; } = true; //Default to true, will update this once IAM is called.

        public static Expression<Func<TeamPermission, TeamUsersReportModel>> Projection()
        {
            return tp => new TeamUsersReportModel
            {
                Id = tp.Id,
                TeamName = tp.Team.Name,
                TeamSlug = tp.Team.Slug,
                Kerb = tp.User.CampusKerberos,
                UserEmail = tp.User.Email,
                UserFirstName = tp.User.FirstName,
                UserLastName = tp.User.LastName,
                RoleName = tp.Role.Name,
            };

        }
    }
}
