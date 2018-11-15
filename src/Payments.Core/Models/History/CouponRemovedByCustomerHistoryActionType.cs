using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Payments.Core.Models.History
{
    public class CouponRemovedByCustomerHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "coupon-removed-customer";

        public string IconClass => "fas fa-ticket-alt text-danger";

        public string GetMessage(string data)
        {
            var d = DeserializeData(data);
            if (d == null)
            {
                return "Coupon code removed by customer.";
            }

            return $"Coupon code \"{d.Code}\" removed by customer.";
        }

        public static DataType DeserializeData(string data)
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
            public string Code { get; set; }
        }
    }
}
