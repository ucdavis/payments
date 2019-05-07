using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payments.Mvc.Models.InvoiceApiViewModels
{
    public class EditInvoiceResult : ApiResult
    {
        public int Id { get; set; }

        public string PaymentLink { get; set; }
    }
}
