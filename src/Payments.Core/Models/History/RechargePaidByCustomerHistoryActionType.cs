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

        public string GetMessage(string data)
        {
            var d = DeserializeData(data);
            if (d == null || d.RechargeAccounts == null || d.RechargeAccounts.Length == 0)
            {
                return "Recharge paid by customer.";
            }

            var sb = new StringBuilder();
            sb.AppendLine("Recharge paid and updated by customer.");

            foreach (var account in d.RechargeAccounts)
            {
                sb.AppendLine($"  Chart: {account.FinancialSegmentString}, Amount: {account.Amount:C}");
                if (!string.IsNullOrWhiteSpace(account.Notes))
                {
                    sb.AppendLine($"    Notes: {account.Notes}");
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
