using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Models.Notifications;
using Serilog;

namespace Payments.Core.Services
{
    public interface INotificationService
    {
        Task SendPaidNotification(PaidNotification notification);

        Task TestWebHook(WebHook webHook);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _dbContext;

        public NotificationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SendPaidNotification(PaidNotification notification)
        {
            // find invoice
            var invoice = await _dbContext.Invoices
                .Include(i => i.Team)
                    .ThenInclude(t => t.WebHooks)
                .FirstOrDefaultAsync(i => i.Id == notification.InvoiceId);

            if (invoice == null)
            {
                return;
            }

            var hooks = invoice.Team.WebHooks.Where(h => h.IsActive && h.TriggerOnPaid);
            foreach (var webHook in hooks)
            {
                await SendWebHookPayload(webHook, notification);
            }
        }

        public async Task TestWebHook(WebHook webHook)
        {
            var notification = new
            {
                name = "test"
            };

            await SendWebHookPayload(webHook, notification);
        }

        private async Task SendWebHookPayload(WebHook webHook, object payload)
        {
            using (var client = new HttpClient())
            {
                var data = JsonConvert.SerializeObject(payload);

                var body = new StringContent(data, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(webHook.Url, body);

                Log.ForContext("webhook", webHook, true)
                   .ForContext("response", response, true)
                   .Information("Sent webhook");
            }
        }
    }
}
