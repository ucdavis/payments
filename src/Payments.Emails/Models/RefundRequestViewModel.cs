using System;
using Payments.Core.Domain;

namespace Payments.Emails.Models
{
    public class RefundRequestViewModel
    {
        public Invoice Invoice { get; set; }
        public PaymentEvent Payment { get; set; }

        public string RefundReason { get; set; }

        public User User { get; set; }
    }
}
