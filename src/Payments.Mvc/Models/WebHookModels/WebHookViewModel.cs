using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Payments.Mvc.Models.WebHookModels
{
    public class WebHookViewModel
    {
        [Required]
        [MaxLength(255)]
        [DisplayName("Payload URL")]
        public string Url { get; set; }

        public string ContentType = "application/json";

        [DisplayName("Enabled")]
        public bool IsActive { get; set; }

        [DisplayName("Trigger on Paid")]
        public bool TriggerOnPaid { get; set; }
    }
}
