using System;

namespace Payments.Core.Models.History
{
    public class InvoiceCreatedHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "invoice-created";

        public string IconClass => "fas fa-file-invoice text-success";

        public string GetMessage(string data)
        {
            return "Invoice was created";
        }
    }
}
