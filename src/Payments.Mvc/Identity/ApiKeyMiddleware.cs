using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Payments.Core.Data;

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
