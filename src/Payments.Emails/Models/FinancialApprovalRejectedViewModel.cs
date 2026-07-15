using Payments.Core.Domain;

namespace Payments.Emails.Models
{
    public class FinancialApprovalRejectedViewModel
    {
        public Invoice Invoice { get; set; }

        public string RejectionReason { get; set; }
    }
}
