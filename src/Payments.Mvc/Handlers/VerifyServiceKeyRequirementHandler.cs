using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Payments.Mvc.Authorization;
using Payments.Mvc.Identity;

namespace Payments.Mvc.Handlers
{
    public class VerifyServiceKeyRequirementHandler : AuthorizationHandler<VerifyServiceKeyRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, VerifyServiceKeyRequirement requirement)
        {
            if (context.User.HasClaim(c => c.Type == ClaimTypes.AuthenticationMethod
                                        && c.Value == ServiceKeyMiddleware.AuthenticationMethodValue))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
