using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Payments.Core.Domain
{
    public class WebHook
    {
        public int Id { get; set; }

        [Required]
        [JsonIgnore]
        public Team Team { get; set; }
        public int TeamId { get; set; }

        [DisplayName("Enabled")]
        public bool IsActive { get; set; }

        [DisplayName("Payload URL")]
        public string Url { get; set; }

        public string ContentType { get; set; }

        [DisplayName("Trigger on Paid")]
        public bool TriggerOnPaid { get; set; }

        [DisplayName("Trigger on Reconcile")]
        public bool TriggerOnReconcile { get; set; }
    }
}
