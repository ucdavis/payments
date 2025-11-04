using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Payments.Core.Domain;

namespace Payments.Core.Models.History
{
    public class RechargeSentToFinancialApproversHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "recharge-sent-financial-approvers";

        public string IconClass => "fas fa-envelope text-info";


        //TODO: So, the get Message might be showing too much info. We could fix that by adding a new method GetDetails(string data) that would be expandable/collapsible in the UI. Would use the ShowDetails to decide whether to show that or not.
        public string GetMessage(string data)
        {
            return "Sent to financial approvers.";
        }

        public bool ShowDetails => true;

        public string GetDetails(string data)
        {
            var d = DeserializeData(data);
            if (d == null || d.FinancialApprovers == null || d.FinancialApprovers.Length == 0)
            {
                return null;
            }

            var sb = new StringBuilder();

            foreach (var approver in d.FinancialApprovers)
            {
                sb.Append($"&nbsp;&nbsp;{approver?.Name}, (<a href=\"mailto:{approver?.Email}\">{approver?.Email}</a>)<br/>");
            }

            return sb.ToString().TrimEnd();
        }

        public DataType DeserializeData(string data)
        {
            try
            {
                return JsonConvert.DeserializeObject<DataType>(data);
            }
            catch
            {
                return null;
            }
        }

        public string SerializeData(DataType data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public class DataType
        {
            public FinancialApprover[] FinancialApprovers { get; set; }
        }

        public class  FinancialApprover
        {
            public string? Email { get; set; }
            public string? Name { get; set; }
        }
    }
}
