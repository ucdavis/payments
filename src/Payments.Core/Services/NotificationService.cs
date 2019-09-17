using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Extensions;
using Payments.Core.Models.Notifications;
using Payments.Core.Models.Webhooks;
using Serilog;

namespace Payments.Core.Services
{
    public interface INotificationService
    {
        Task SendPaidNotification(PaidNotification notification);

        Task SendReconcileNotification(ReconcileNotification notification);

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

            // send webhook notifications
            var hooks = invoice.Team.WebHooks.Where(h => h.IsActive && h.TriggerOnPaid);
            var payload = new PaidPayload()
            {
                InvoiceId = invoice.Id,
                PaidOn = invoice.PaidAt.ToPacificTime().Value,
            };
            foreach (var webHook in hooks)
            {
                await SendWebHookPayload(webHook, payload);
            }
        }

        public async Task SendReconcileNotification(ReconcileNotification notification)
        {
            // find invoice
            var invoice = await _dbContext.Invoices
                .FirstOrDefaultAsync(i => i.Id == notification.InvoiceId);

            if (invoice == null)
            {
                return;
            }

            // send webhook notifications
            var hooks = invoice.Team.WebHooks.Where(h => h.IsActive && h.TriggerOnReconcile);
            var payload = new ReconcilePayload()
            {
                InvoiceId = invoice.Id,
            };
            foreach (var webHook in hooks)
            {
                await SendWebHookPayload(webHook, payload);
            }
        }

        public async Task TestWebHook(WebHook webHook)
        {
            var payload = new TestPayload()
            {
                HookId = webHook.Id,
                HookActive = webHook.IsActive,
            };
            await SendWebHookPayload(webHook, payload);
        }

        private async Task SendWebHookPayload(WebHook webHook, WebhookPayload payload)
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
