using System;
using System.Collections.Generic;
using System.Text;

namespace Payments.Core.Models.History
{
    public class MarkCompletedHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "mark-completed";

        public string IconClass => "fas fa-check-circle text-success";

        public string GetMessage(string data)
        {
            return "Invoice was marked completed";
        }
    }
}
