using Payments.Core.Domain;
using System;
using System.Linq.Expressions;

namespace Payments.Mvc.Models.ReportViewModels
{
    public class TeamUsersReportModel
    {
        public int Id { get; set; }
        public string TeamName { get; set; }
        public string TeamSlug { get; set; }
        public string Kerb { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
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
                UserName = tp.User.Name,
                RoleName = tp.Role.Name,
            };

        }
    }
}
