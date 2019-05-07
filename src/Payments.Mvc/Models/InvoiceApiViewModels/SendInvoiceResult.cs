using System;

namespace Payments.Mvc.Models.InvoiceApiViewModels
{
    public class SendInvoiceResult : ApiResult
    {
        public int Id { get; set; }

        public string PaymentLink { get; set; }
    }
}
