using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payments.Mvc.Models.InvoiceApiViewModels
{
    public class CreateInvoiceResult : ApiResult
    {
        public int[] Ids { get; set; }
    }
}
