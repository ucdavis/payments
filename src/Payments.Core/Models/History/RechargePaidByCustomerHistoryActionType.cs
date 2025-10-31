using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Payments.Core.Domain;

namespace Payments.Core.Models.History
{
    public class RechargePaidByCustomerHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "recharge-paid-customer";

        public string IconClass => "fas fa-check-circle text-success";


        //TODO: So, the get Message might be showing too much info. We could fix that by adding a new method GetDetails(string data) that would be expandable/collapsible in the UI. Would use the AllowRaw to decide whether to show that or not.
        public string GetMessage(string data)
        {
            var d = DeserializeData(data);
            if (d == null || d.RechargeAccounts == null || d.RechargeAccounts.Length == 0)
            {
                return "Recharge paid by customer.";
            }

            var sb = new StringBuilder();
            sb.Append("Recharge paid by customer.<br/>");

            foreach (var account in d.RechargeAccounts)
            {
                sb.Append($"&nbsp;&nbsp;Chart: {account.FinancialSegmentString}, Amount: {account.Amount:C}<br/>");
                if (!string.IsNullOrWhiteSpace(account.Notes))
                {
                    sb.Append($"&nbsp;&nbsp;&nbsp;&nbsp;Notes: {account.Notes}<br/>");
                }
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
            public RechargeAccount[] RechargeAccounts { get; set; }
        }
    }
}
