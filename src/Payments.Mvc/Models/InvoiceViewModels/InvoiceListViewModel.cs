using Payments.Core.Domain;
using System.Collections.Generic;

namespace Payments.Mvc.Models.InvoiceViewModels
{
    public class InvoiceListViewModel
    {
        public IList<Invoice> Invoices { get; set; }

        public InvoiceFilterViewModel Filter { get; set; }

        public int? Year { get;set;}
    }
}
