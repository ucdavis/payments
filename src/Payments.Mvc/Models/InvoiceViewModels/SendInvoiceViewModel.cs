using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payments.Mvc.Models.InvoiceViewModels
{
    public class SendInvoiceViewModel
    {
        public string ccEmails { get; set; }

        public string bccEmails { get; set; }
    }
}
