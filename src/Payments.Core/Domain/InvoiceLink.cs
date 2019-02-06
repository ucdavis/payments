using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Core.Domain
{
    public class InvoiceLink
    {
        [Key]
        public int Id { get; set; }

        public string LinkId { get; set; }

        public Invoice Invoice { get; set; }

        public bool Expired { get; set; }
    }
}
