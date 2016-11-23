using System;

namespace Payments.Core.Models
{
    public class Invoice : DomainObject
    {
        public string Title { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
