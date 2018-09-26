using System;
using System.Collections.Generic;
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

        public bool IsActive { get; set; }

        public string Url { get; set; }

        public string ContentType { get; set; }

        public bool TriggerOnPaid { get; set; }
    }
}
