using System;
using System.Collections.Generic;
using System.Text;

namespace Payments.Core.Models.Configuration
{
    public class SparkpostSettings
    {
        public string ApiKey { get; set; }
        public object BaseUrl { get; internal set; }
    }
}
