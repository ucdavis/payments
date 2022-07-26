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
        public bool RequireKfsAccount { get; set; } = true; //We can turn this off and or remove when we switch over to AE
    }
}
