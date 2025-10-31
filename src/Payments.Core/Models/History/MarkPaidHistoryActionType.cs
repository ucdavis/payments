using System;
using System.Collections.Generic;
using System.Text;

namespace Payments.Core.Models.History
{
    public class MarkPaidHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "mark-paid";

        public string IconClass => "fas fa-check-circle text-success";

        public string GetMessage(string data)
        {
            return "Invoice was marked paid";
        }
        public string GetDetails(string data)
        {
            return null;
        }
    }
}
