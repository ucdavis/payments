using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Payments.Core.Domain;

namespace Payments.Mvc.Models.ReportViewModels
{
    public class TaxReportViewModel
    {
        public TaxReportViewModel()
        {
            Timespan = "month";
        }

        [Display(Name = "StartDate")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Timespan")]
        public string Timespan { get; set; }

        public IList<Invoice> Invoices { get; set; }
    }
}
