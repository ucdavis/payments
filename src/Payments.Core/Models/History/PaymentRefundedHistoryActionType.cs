using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Payments.Core.Models.History
{
    public class PaymentRefundedHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "payment-refunded";

        public string IconClass => "fas fa-check-circle text-success";

        public string GetMessage(string data)
        {
            var d = DeserializeData(data);
            if (d == null)
            {
                return "Payment Refunded";
            }

            return $"{d.Amount:C2} payment refunded.";
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

        public static string SerializeData(DataType data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public class DataType
        {
            public decimal Amount { get; set; }
        }
    }
}
