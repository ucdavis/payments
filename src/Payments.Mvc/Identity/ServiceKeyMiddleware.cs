using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Payments.Core.Models.Configuration;

namespace Payments.Mvc.Identity
{
    public class ServiceKeyMiddleware
    {
        public const string HeaderKey = "X-Auth-Token";
        public const string AuthenticationMethodValue = "ServiceKey";

        private readonly RequestDelegate _next;
        private ILogger _logger;

        public ServiceKeyMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<ApiKeyMiddleware>();
        }

        public Task Invoke(HttpContext context, IOptions<PaymentsApiSettings> settings)
        {
            // check for header
            if (!context.Request.Headers.ContainsKey(HeaderKey))
            {
                return _next(context);
            }
            var headerValue = context.Request.Headers[HeaderKey].FirstOrDefault();

            // check that header matches
            if (!string.Equals(headerValue, settings.Value.ApiKey, StringComparison.OrdinalIgnoreCase))
            {
                return _next(context);
            }

            context.User.AddIdentity(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.AuthenticationMethod, AuthenticationMethodValue),
            }));

            return _next(context);
        }
    }
}
