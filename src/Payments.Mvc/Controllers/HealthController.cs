using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;

namespace Payments.Mvc.Controllers
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HealthController(ApplicationDbContext context) => _context = context;

        [HttpGet("/health")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(5));
            var healthy = false;
            try
            {
                healthy = await _context.Database.CanConnectAsync(timeout.Token);
            }
            catch (OperationCanceledException)
            {
                // A timed-out dependency is unavailable; never expose connection details.
            }

            return new ContentResult
            {
                StatusCode = healthy ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable,
                ContentType = "text/plain",
                Content = healthy ? "Healthy" : "Unhealthy",
            };
        }
    }
}
