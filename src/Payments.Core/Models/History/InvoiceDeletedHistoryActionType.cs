using System;
using System.Collections.Generic;
using System.Text;

namespace Payments.Core.Models.History
{
    public class InvoiceDeletedHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "invoice-deleted";

        public string IconClass => "fas fa-file-invoice text-danger";

        public string GetMessage(string data)
        {
            return "Invoice was deleted";
        }
        public string GetDetails(string data)
        {
            return null;
        }
    }
}
