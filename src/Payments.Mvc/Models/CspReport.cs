using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Payments.Mvc.Models
{
    public class CspReportRequest
    {
        [Newtonsoft.Json.JsonProperty("csp-report")]
        public CspReport CspReport { get; set; }
    }
    
    public class CspReport
    {
        [JsonProperty("blocked-uri")]
        public string BlockedUri { get; set; }

        [JsonProperty("document-uri")]
        public string DocumentUri { get; set; }

        [JsonProperty("original-policy")]
        public string OriginalPolicy { get; set; }

        [JsonProperty("referrer")]
        public string Referrer { get; set; }

        [JsonProperty("violated-directive")]
        public string ViolatedDirective { get; set; }
    }
}
