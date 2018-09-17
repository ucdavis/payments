using System;

namespace Payments.Core.Models.History
{
    public class InvoiceUnlockedHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "invoice-unlocked";

        public string IconClass => "fas fa-unlock text-warning";

        public string GetMessage(string data)
        {
            return "Invoice was unlocked";
        }
    }
}
