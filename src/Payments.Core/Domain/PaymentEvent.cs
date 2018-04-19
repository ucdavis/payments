using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Payments.Core.Domain
{
    public class PaymentEvent
    {
        public PaymentEvent()
        {
            OccuredAt = DateTime.UtcNow;
        }
        [Key]
        public string Transaction_Id { get; set; }

        /// <summary>
        /// Unique merchant-generated order reference or tracking number for each transaction.
        /// (Order Id)
        /// </summary>
        public int Req_Reference_Number { get; set; }

        public string Decision { get; set; }

        public int Reason_Code { get; set; }
        public string Auth_Amount { get; set; }

        /// <summary>
        /// Json of what CyberSource returned including fields above
        /// </summary>
        public string ReturnedResults { get; set; }
        public DateTime OccuredAt { get; set; }
    }
}
