using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Payments.Core.Models.History
{
    public class CouponAddedByCustomerHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "coupon-added-customer";

        public string IconClass => "fas fa-ticket-alt text-info";

        public string GetMessage(string data)
        {
            var d = DeserializeData(data);
            if (d == null)
            {
                return "Coupon code added by customer.";
            }

            return $"Coupon code \"{d.Code}\" added by customer.";
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
            public string Code { get; set; }
        }
    }
}
