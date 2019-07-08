using System;
using System.ComponentModel;
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

        [MaxLength(60)]
        [DisplayName("Billing First Name")]
        public string BillingFirstName { get; set; }

        [MaxLength(60)]
        [DisplayName("Billing Last Name")]
        public string BillingLastName { get; set; }

        [MaxLength(255)]
        [DisplayName("Billing Email")]
        public string BillingEmail { get; set; }

        [MaxLength(40)]
        [DisplayName("Billing Company")]
        public string BillingCompany { get; set; }

        [MaxLength(15)]
        [DisplayName("Billing Phone")]
        public string BillingPhone { get; set; }

        [MaxLength(60)]
        [DisplayName("Billing Street")]
        public string BillingStreet1 { get; set; }

        [MaxLength(60)]
        [DisplayName("Billing Street 2")]
        public string BillingStreet2 { get; set; }

        [MaxLength(50)]
        [DisplayName("Billing City")]
        public string BillingCity { get; set; }

        [MaxLength(2)]
        [DisplayName("Billing State")]
        public string BillingState { get; set; }

        [MaxLength(2)]
        [DisplayName("Billing Country")]
        public string BillingCountry { get; set; }

        [MaxLength(10)]
        [DisplayName("Billing Postal Code")]
        public string BillingPostalCode { get; set; }

        [MaxLength(3)]
        public string CardType { get; set; }

        [MaxLength(20)]
        public string CardNumber { get; set; }

        public DateTime? CardExpiry { get; set; }

        /// <summary>
        /// Json of what CyberSource returned including fields above
        /// </summary>
        [JsonIgnore]
        public string ReturnedResults { get; set; }

        public DateTime OccuredAt { get; set; }

        public Invoice Invoice { get; set; }
    }
}
