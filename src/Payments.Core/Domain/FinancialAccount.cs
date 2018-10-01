using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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

        /// <summary>
        /// Chart Code.
        /// </summary>
        [StringLength(1)]
        [Required]
        public string Chart { get; set; }

        /// <summary>
        /// Account used in the general ledger.
        /// Accounts are specific to a Chart Code.
        /// </summary>
        [StringLength(7)]
        [RegularExpression("[A-Z0-9]*")]
        [Required]
        public string Account { get; set; }

        /// <summary>
        /// Sub-Account is an optional accounting unit attribute.
        /// Chart Code and Account are part of Sub-Account key.
        /// </summary>
        [StringLength(5)]
        [DisplayFormat(NullDisplayText = "-----")]
        public string SubAccount { get; set; }

        [StringLength(9)]
        [DisplayFormat(NullDisplayText = "---------")]
        public string Project { get; set; }

        [DisplayName("Default")]
        public bool IsDefault { get; set; }

        [DisplayName("Active")]
        public bool IsActive { get; set; } = true;
        
        [Required]
        [JsonIgnore]
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
    }
}
