using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Payments.Core.Domain
{
    public class History
    {
        public History()
        {
            ActionDateTime = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public DateTime ActionDateTime { get; set; }

        public string Data { get; set; }

        public User Actor { get; set; }

        public string ActorId { get; set; }
    }
}
