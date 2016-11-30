using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Payments.Core.Models;
using Payments.Models;

namespace Payments.Tests.Helpers
{
    public static class CreateValidEntities
    {
        public static Invoice Invoice(int? counter, bool populateAllFields = false)
        {
            var rtValue = new Invoice();
            rtValue.ClientName = String.Format("ClientName{0}", counter);
            rtValue.ClientEmail = String.Format("clientemail{0}@test.com", counter);
            rtValue.Status = InvoiceStatus.Created;
            rtValue.LineItems = new List<LineItem>();            
            rtValue.Title = string.Format("Title{0}", counter);
            if (populateAllFields)
            {
                rtValue.TotalAmount = counter.HasValue? counter.Value: 99.9m;
                rtValue.Scrubbers = new List<Scrubber>();
                rtValue.History = new List<History>();
            }
            rtValue.Id = counter ?? 99;

            return rtValue;
        }

        public static InvoiceEditViewModel InvoiceEditViewModel(int? counter, bool populateAllFields = false)
        {
            var rtValue = new InvoiceEditViewModel();
            rtValue.Title = string.Format("Title{0}", counter);
            if (populateAllFields)
            {
                rtValue.TotalAmount = counter.HasValue ? counter.Value : 99.9m;
            }

            return rtValue;
        }
    }
}
