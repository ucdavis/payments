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
        
        public bool UseCoa { get; set; } = false;
        //If UseCoa is turned on, ShowCoa should be as well.
        public bool ShowCoa { get; set; } = false;

        public string ClearingFinancialSegmentString { get; set; }
        public string FeeFinancialSegmentString { get; set; }
    }
}
