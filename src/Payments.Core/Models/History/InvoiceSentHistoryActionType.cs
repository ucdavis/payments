using System;

namespace Payments.Core.Models.History
{
    public class InvoiceSentHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "invoice-sent";

        public string IconClass => "fas fa-envelope text-info";

        public string GetMessage(string data)
        {
            return "Invoice was sent to the customer";
        }
    }
}
