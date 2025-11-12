using System;
using System.Collections.Generic;
using System.Text;

namespace Payments.Core.Models.Configuration
{
    public class PaymentsApiSettings
    {
        public string BaseUrl { get; set; }

        public string ApiKey { get; set; }

        public string RechargeApiKey { get; set; }

        public string RechargeSourceName { get; set; }
    }
}
