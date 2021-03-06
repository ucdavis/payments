﻿using System;
using Payments.Core.Domain;

namespace Payments.Mvc.Models.FinancialModels
{
    public class FinancialAccountDetailsModel
    {
        public FinancialAccount FinancialAccount { get; set; }

        public KfsAccount KfsAccount { get; set; }

        public bool IsAccountValid { get; set; }

        public bool? IsProjectValid { get; set; } = null;
    }
}
