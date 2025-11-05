using System;

namespace Payments.Core.Models.History
{
    public class RechargeRejectedByFinancialApproverHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "recharge-rejected-financial-approver";

        public string IconClass => "fas fa-times-circle text-danger";

        public string GetMessage(string data)
        {
            return "Rejected by financial approver.";
        }

        public bool ShowDetails => true;

        public string GetDetails(string data)
        {
            return data;
        }
    }
}
