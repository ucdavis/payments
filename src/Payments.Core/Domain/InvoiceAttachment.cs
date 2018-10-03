using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Core.Domain
{
    public class InvoiceAttachment
    {
        [Key]
        public int Id { get; set; }

        public string Identifier { get; set; }

        public string Description { get; set; }

        public string ContentType { get; set; }

        public int Size { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public int TeamId { get; set; }
    }
}
