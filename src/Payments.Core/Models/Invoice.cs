using System;

namespace Payments.Core.Models
{
    public class Invoice : DomainObject
    {
        public decimal TotalAmount { get; set; }
    }
}
