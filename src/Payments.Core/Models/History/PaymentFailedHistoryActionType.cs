using System;

namespace Payments.Core.Models.History
{
    public class PaymentFailedHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "payment-failed";

        public string IconClass => "fas fa-times-circle text-danger";

        public string GetMessage(string data)
        {
            return "Payment Failed";
        }
        public string GetDetails(string data)
        {
            return null;
        }
    }
}
