using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using Payments.Core.Data;
using Payments.Mvc.Controllers;

namespace Payments.Mvc.Identity
{
    public class ApiKeyMiddleware
    {
        public const string HeaderKey = "Authorization";
        public const string AuthenticationMethodValue = "ApiKey";

        private readonly RequestDelegate _next;
        private ILogger _logger;

        public ApiKeyMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<ApiKeyMiddleware>();
        }

        public Task Invoke(HttpContext context, ApplicationDbContext dbContext)
        {
            var isTeamEndpoint = context.GetEndpoint()?.Metadata
                .GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(TeamsApiController);

            // The team lookup requires an API key even when the caller has a login cookie.
            // Explicit empty responses bypass the HTML status-code handler.
            if (isTeamEndpoint && string.IsNullOrWhiteSpace(context.Request.Headers[HeaderKey].FirstOrDefault()))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentLength = 0;
                return Task.CompletedTask;
            }

            // check for header
            if (!context.Request.Headers.ContainsKey(HeaderKey))
            {
                return _next(context);
            }
            var headerValue = context.Request.Headers[HeaderKey].FirstOrDefault();

            // lookup apikey from db
            var team = dbContext.Teams
                .FirstOrDefault(a => a.ApiKey == headerValue);

            if (team == null || !team.IsActive)
            {
                if (isTeamEndpoint)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentLength = 0;
                    return Task.CompletedTask;
                }

                return _next(context);
            }

            context.User.AddIdentity(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Sid, team.Id.ToString()), 
                new Claim(ClaimTypes.Name, team.Name),
                new Claim(ClaimTypes.AuthenticationMethod, AuthenticationMethodValue),
            }));

            return _next(context);
        }
    }
}
