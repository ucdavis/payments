using Microsoft.AspNetCore.Authorization;

namespace Payments.Mvc.Authorization
{
    public class VerifyTeamPermission : IAuthorizationRequirement
    {
        public readonly object[] Roles;

        public VerifyTeamPermission(params object[] roles)
        {
            Roles = roles;
        }
    }
}
