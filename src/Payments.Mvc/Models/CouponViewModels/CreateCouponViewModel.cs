using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payments.Mvc.Models.CouponViewModels
{
    public class CreateCouponViewModel
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? DiscountPercent { get; set; }

        public DateTime? ExpiresAt { get; set; }
    }
}
