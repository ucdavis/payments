using System;
using Newtonsoft.Json;

namespace Payments.Core.Models.History
{
    public class PaymentCompletedHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "payment-completed";

        public string IconClass => "fas fa-check-circle text-success";

        public string GetMessage(string data)
        {
            var d = DeserializeData(data);
            if (d == null)
            {
                return "Payment Successful";
            }

            return $"{d.Amount:C2} payment successful";
        }

        public string GetDetails(string data)
        {
            return null;
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
            public decimal Amount { get; set; }
        }
    }
}
