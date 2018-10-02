using System;
using System.Collections.Generic;
using Payments.Core.Domain;

namespace Payments.Mvc.Models.SearchViewModels
{
    public class SearchResultsViewModel
    {
        public IList<Invoice> Invoices { get; set; }
    }
}
