using System;
using System.Collections.Generic;

namespace Payments.Mvc.Models.PaymentViewModels
{
    public class PaymentInvoiceViewModel : BaseInvoiceViewModel
    {
        public string LinkId { get; set; }

        public Dictionary<string, string> PaymentDictionary { get; set; }
    }
}
