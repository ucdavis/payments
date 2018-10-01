using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Payments.Core.Models.Notifications
{
    public class PaidNotification
    {
        public static string Name = "invoice_paid";

        public int InvoiceId { get; set; }
    }
}
