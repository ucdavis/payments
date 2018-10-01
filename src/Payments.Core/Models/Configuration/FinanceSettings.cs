using System;
using System.Collections.Generic;
using System.Text;

namespace Payments.Core.Models.Configuration
{
    public class FinanceSettings
    {
        public string ClearingChart { get; set; }
        public string ClearingAccount { get; set; }
        public string FeeChart { get; set; }
        public string FeeAccount { get; set; }
    }
}
