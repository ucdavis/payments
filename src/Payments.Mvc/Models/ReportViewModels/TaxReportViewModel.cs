using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Payments.Core.Domain;

namespace Payments.Mvc.Models.ReportViewModels
{
    public class TaxReportViewModel
    {
        [Display(Name = "Fiscal Year")]
        public int FiscalYear { get; set; }

        public IList<Invoice> Invoices { get; set; }
    }
}
