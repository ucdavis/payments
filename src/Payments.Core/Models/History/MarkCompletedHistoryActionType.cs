using System;
using System.Collections.Generic;
using System.Text;

namespace Payments.Core.Models.History
{
    public class SetBackToPaidHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "mark-completed";

        public string IconClass => "fas fa-check-circle text-success";

        public string GetMessage(string data)
        {
            return "Invoice set back to paid to generate disbursements";
        }
        public string GetDetails(string data)
        {
            return null;
        }
    }
}
