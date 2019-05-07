using System;
using System.Collections.Generic;
using System.Text;

namespace Payments.Core.Models.Webhooks
{
    public class PaidPayload : WebhookPayload
    {
        public override string Action => "invoice_paid";

        public int InvoiceId { get; set; }

        public DateTime PaidOn { get; set; }
    }
}
