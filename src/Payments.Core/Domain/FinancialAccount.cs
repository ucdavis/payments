using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;

namespace Payments.Core.Domain
{
    public class FinancialAccount
    {
        [Key]
        public int Id { get; set; }

        [StringLength(128)]
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [StringLength(1)]
        [Required]
        public string Chart { get; set; }

        [StringLength(7)]
        [Required]
        public string Account { get; set; }

        [StringLength(4)]
        [Required]
        public string Object { get; set; }

        [StringLength(5)]
        [DisplayFormat(NullDisplayText = "-----")]
        public string SubAccount { get; set; }

        [StringLength(3)]
        [DisplayFormat(NullDisplayText = "---")]
        public string SubObject { get; set; }

        [StringLength(9)]
        [DisplayFormat(NullDisplayText = "---------")]
        public string Project { get; set; }

        [DisplayName("Default")]
        public bool IsDefault { get; set; }

        [DisplayName("Active")]
        public bool IsActive { get; set; } = true;

        public Team Team { get; set; }
        public int TeamId { get; set; }

        public string GetAccountString()
        {
            if (string.IsNullOrWhiteSpace(SubAccount))
            {
                return $"{Chart}-{Account}";
            }

            return $"{Chart}-{Account}-{SubAccount}";
        }

        public string GetObjectString()
        {
            if (string.IsNullOrWhiteSpace(SubObject))
            {
                return Object;
            }

            return $"{Object}-{SubObject}";
        }
    }
}
