using System;
using Payments.Core.Domain;

namespace Payments.Emails.Models
{
    public class RefundRequestViewModel
    {
        public Invoice Invoice { get; set; }
        public PaymentEvent Payment { get; set; }
    }
}
