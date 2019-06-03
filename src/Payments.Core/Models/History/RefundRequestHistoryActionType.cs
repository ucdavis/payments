using System;
using System.Collections.Generic;
using System.Text;

namespace Payments.Core.Models.History
{
    public class RefundRequestHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "refund-requested";

        public string IconClass => "fas fa-hand-holding-usd text-warning";

        public string GetMessage(string data)
        {
            return "Refund Requested.";
        }
    }
}
