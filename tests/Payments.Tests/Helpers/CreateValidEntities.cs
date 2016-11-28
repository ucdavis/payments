using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Payments.Core.Models;

namespace Payments.Tests.Helpers
{
    public static class CreateValidEntities
    {
        public static Invoice Invoice(int? counter)
        {
            var rtValue = new Invoice();
            rtValue.Title = string.Format("Title{0}", counter);
            rtValue.Id = counter ?? 99;

            return rtValue;
        }
    }
}
