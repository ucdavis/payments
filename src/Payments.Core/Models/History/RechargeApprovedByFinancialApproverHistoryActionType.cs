using System;

namespace Payments.Core.Models.History
{
    public class RechargeApprovedByFinancialApproverHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "recharge-approved-financial-approver";

        public string IconClass => "fas fa-check-circle text-success";

        public string GetMessage(string data)
        {
            return "Approved by all financial approvers.";
        }

        public bool ShowDetails => true; //So we can have notes if it is auto approved.

        public string GetDetails(string data)
        {
            return data;
        }
    }
}
