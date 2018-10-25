using System;
using System.Collections.Generic;

namespace Payments.Mvc.Models.InvoiceViewModels
{
    public class InvoiceFilterViewModel
    {
        public InvoiceFilterViewModel()
        {
            Statuses = new List<string>();
        }

        public IList<string> Statuses { get; set; }

        public bool ShowDeleted { get; set; }

        public DateTime? CreatedDateStart { get; set; }

        public DateTime? CreatedDateEnd { get; set; }
    }
}
