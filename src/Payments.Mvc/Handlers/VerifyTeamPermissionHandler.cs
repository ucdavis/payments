using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Payments.Core.Domain;
using Payments.Mvc.Authorization;
using Payments.Mvc.Identity;
using Payments.Mvc.Models.Roles;

namespace Payments.Mvc.Handlers
{
    public class VerifyTeamPermissionHandler : AuthorizationHandler<VerifyTeamPermission>
    {
        private readonly ApplicationUserManager _userManager;

        public VerifyTeamPermissionHandler(ApplicationUserManager userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, VerifyTeamPermission requirement)
        {
            if (context.User.IsInRole(ApplicationRoleCodes.Admin))
            {
                context.Succeed(requirement);
                return;
            }
            var team = "";
            if (context.Resource is AuthorizationFilterContext mvcContext)
            {
                if (mvcContext.RouteData.Values["team"] != null)
                {
                    team = mvcContext.RouteData.Values["team"].ToString();
                }
            }

            var user = await _userManager.GetUserAsync(context.User);            
            if (user != null && team != "")
            {
                var permissions = user.TeamPermissions
                    .Where(p => p.Team.Slug == team)
                    .Where(p => requirement.Roles.Contains(p.Role.Name));

                if (permissions.Any())
                {
                    context.Succeed(requirement);
                }
            }

            // TODO: Check for system admin role
        }
    }
}
