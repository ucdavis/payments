using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Payments.Core.Domain
{
    public class PaymentEvent
    {
        public PaymentEvent()
        {
            OccuredAt = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }

        public string Processor { get; set; }

        public string ProcessorId { get; set; }

        public string Decision { get; set; }

        public decimal Amount { get; set; }

        /// <summary>
        /// Json of what CyberSource returned including fields above
        /// </summary>
        [JsonIgnore]
        public string ReturnedResults { get; set; }

        public DateTime OccuredAt { get; set; }

        public Invoice Invoice { get; set; }
    }
}
