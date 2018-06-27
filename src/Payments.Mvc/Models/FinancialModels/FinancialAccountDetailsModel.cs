using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Payments.Core.Domain;

namespace Payments.Mvc.Models.FinancialModels
{
    public class FinancialAccountDetailsModel
    {
        public FinancialAccount FinancialAccount { get; set; }
        public KfsAccount KfsAccount { get; set; }

        public bool IsAccountValid { get; set; }
        public bool IsObjectValid { get; set; }
        public bool? IsSubObjectValid { get; set; } = null;
        public bool? IsProjectValid { get; set; } = null;
    }
}
