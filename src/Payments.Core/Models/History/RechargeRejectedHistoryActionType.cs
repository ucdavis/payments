using System;

namespace Payments.Core.Models.History
{
    public class RechargeRejectedHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "recharge-rejected";

        public string IconClass => "fas fa-times-circle text-danger";

        public string GetMessage(string data)
        {
            return "Rejected.";
        }

        public bool ShowDetails => true;

        public string GetDetails(string data)
        {
            return data;
        }
    }
}
