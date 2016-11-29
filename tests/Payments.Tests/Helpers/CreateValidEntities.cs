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
            rtValue.Title = string.Format("Title{0}", counter);
            if (populateAllFields)
            {
                rtValue.TotalAmount = counter.HasValue? counter.Value: 99.9m;
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
