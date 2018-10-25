using System;
using System.Collections.Generic;
using System.Text;

namespace Payments.Core.Models.Storage
{
    public class SasResponse
    {
        public string Url { get; set; }

        public string AccessUrl { get; set; }

        public string Identifier { get; set; }
    }
}
