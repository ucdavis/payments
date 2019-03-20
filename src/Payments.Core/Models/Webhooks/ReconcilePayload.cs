using System;
using System.Collections.Generic;
using System.Text;

namespace Payments.Core.Models.Webhooks
{
    public class ReconcilePayload : WebhookPayload
    {
        public override string Action => "invoice_reconciled";

        public int InvoiceId { get; set; }

        public DateTime ReconciledOn { get; set; }
    }
}
