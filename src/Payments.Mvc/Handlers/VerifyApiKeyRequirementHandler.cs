using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Payments.Mvc.Authorization;
using Payments.Mvc.Identity;

namespace Payments.Mvc.Handlers
{
    public class VerifyApiKeyRequirementHandler : AuthorizationHandler<VerifyApiKeyRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, VerifyApiKeyRequirement requirement)
        {
            if (context.User.HasClaim(c => c.Type == ClaimTypes.AuthenticationMethod && c.Value == ApiKeyMiddleware.AuthenticationMethodValue))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
