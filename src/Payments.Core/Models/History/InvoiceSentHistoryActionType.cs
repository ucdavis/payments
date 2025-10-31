using Newtonsoft.Json;
using Payments.Core.Domain;
using System;
using System.Text;

namespace Payments.Core.Models.History
{
    public class InvoiceSentHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "invoice-sent";

        public string IconClass => "fas fa-envelope text-info";

        public string GetMessage(string data)
        {
            return "Invoice was sent to the customer";
        }

        public bool ShowDetails => true; // But only for recharges

        public string GetDetails(string data)
        {
            var d = DeserializeData(data);
            if (d == null || d.RechargeAccounts == null || d.RechargeAccounts.Length == 0)
            {
                return null;
            }

            var sb = new StringBuilder();

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
