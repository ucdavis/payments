using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Payments.Core.Domain
{
    public class History
    {
        [Key]
        public int Id { get; set; }

        public string Type { get; set; }

        public string Data { get; set; }
        public DateTime Date { get; set; }
        public User Actor { get; set; }
        public string ActorId { get; set; }
    }
}
