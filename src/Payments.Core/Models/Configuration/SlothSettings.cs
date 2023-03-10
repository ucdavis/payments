using System;
using System.Collections.Generic;
using System.Text;

namespace Payments.Core.Models.Configuration
{
    public class SlothSettings
    {
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
        public string TransactionLookup { get; set; }
    }
}
