using System;
using System.Collections.Generic;
using System.Text;

namespace Payments.Core.Models.Webhooks
{
    public class TestPayload : WebhookPayload
    {
        public override string Action => "ping";

        public int HookId { get; set; }

        public bool HookActive { get; set; }
    }
}
