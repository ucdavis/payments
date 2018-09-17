using System;

namespace Payments.Core.Models.History
{
    public class InvoiceClosedHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "invoice-closed";

        public string IconClass => "fas fa-file-invoice text-success";

        public string GetMessage(string data)
        {
            return "Invoice was closed";
        }
    }
}
