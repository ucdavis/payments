using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Payments.Web.Infrastructure
{
    public class RequestContextMiddleware
    {
        /// <summary>
        /// The property name carrying the request ID.
        /// </summary>
        public const string DefaultRequestIdPropertyName = "HttpRequestId";

        private readonly RequestDelegate _next;
        private readonly string _propertyName;

        /// <summary>
        /// Construct the middleware.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="propertyName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public RequestContextMiddleware(RequestDelegate next, string propertyName = DefaultRequestIdPropertyName)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            _next = next;
            _propertyName = string.IsNullOrWhiteSpace(propertyName) ? DefaultRequestIdPropertyName : propertyName;
        }

        /// <summary>
        /// Process a request.
        /// </summary>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            // There is not yet a standard way to uniquely identify and correlate an owin request
            // ... hence 'RequestId' https://github.com/owin/owin/issues/21
            using (LogContext.PushProperty(_propertyName, Guid.NewGuid()))
            {
                await _next(context);
            }
        }
    }
}
