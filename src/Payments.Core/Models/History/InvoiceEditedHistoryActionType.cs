using System;

namespace Payments.Core.Models.History
{
    public class InvoiceEditedHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "invoice-edited";

        public string IconClass => "far fa-edit text-primary";

        public string GetMessage(string data)
        {
            return "Invoice was edited";
        }
        public string GetDetails(string data)
        {
            return null;
        }
    }
}
