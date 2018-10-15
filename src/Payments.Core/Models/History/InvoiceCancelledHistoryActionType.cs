using System;

namespace Payments.Core.Models.History
{
    public class InvoiceCancelledHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "invoice-cancelled";

        public string IconClass => "fas fa-file-invoice text-danger";

        public string GetMessage(string data)
        {
            return "Invoice was cancelled";
        }
    }
}
