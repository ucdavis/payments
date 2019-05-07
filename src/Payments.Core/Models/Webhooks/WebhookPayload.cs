using System;

namespace Payments.Core.Models.Webhooks
{
    public abstract class WebhookPayload
    {
        public abstract string Action { get; }
    }
}
