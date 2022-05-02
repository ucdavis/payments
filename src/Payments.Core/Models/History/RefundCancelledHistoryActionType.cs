using System;
using System.Collections.Generic;
using System.Text;

namespace Payments.Core.Models.History
{
    public class RefundCancelledHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "refund-cancelled";

        public string IconClass => "fas fa-ban text-warning";

        public string GetMessage(string data)
        {
            //This has data (reason), but I'm showing it differently
            return "Refund Request Cancelled.";
        }
    }
}
