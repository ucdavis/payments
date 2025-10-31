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
            //This has data (refund reason), but I'm showing it differently
            return "Refund Requested.";
        }
        public string GetDetails(string data)
        {
            return null;
        }
    }
}
