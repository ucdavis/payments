using System;

namespace Payments.Core.Models.History
{
    public class RechargeCompletedInSlothHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "recharge-completed-in-sloth";

        public string IconClass => "fas fa-check-circle text-success";

        public string GetMessage(string data)
        {
            return "Money moved.";
        }

        public bool ShowDetails => false; 

        public string GetDetails(string data)
        {
            return data;
        }
    }
}
