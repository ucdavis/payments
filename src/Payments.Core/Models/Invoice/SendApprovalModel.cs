using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Core.Models.Invoice
{
    public class SendApprovalModel
    {
        public EmailRecipient[] emails { get; set; }
        public string bccEmails { get; set; }
    }

    public class EmailRecipient
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }
}
