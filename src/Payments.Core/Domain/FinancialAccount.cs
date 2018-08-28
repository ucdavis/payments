using System;
using System.ComponentModel.DataAnnotations;

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
        /// Object codes represent all income, expense, asset, liability and fund balance classification
        ///  that are assigned to transactions and help identify the nature of the transaction.
        /// Object Codes are specific to a Chart Code.
        /// </summary>
        [StringLength(4)]
        [Required]
        public string Object { get; set; }

        /// <summary>
        /// Sub-Account is an optional accounting unit attribute.
        /// Chart Code and Account are part of Sub-Account key.
        /// </summary>
        [StringLength(5)]
        [DisplayFormat(NullDisplayText = "-----")]
        public string SubAccount { get; set; }

        /// <summary>
        /// Sub-Object is an optional accounting unit attribute that allows finer 
        ///  distinctions within a particular object code on an account.
        /// Sub-Object codes are specific to a Chart Code, Account and Object Code combination.
        /// </summary>
        [StringLength(3)]
        [DisplayFormat(NullDisplayText = "---")]
        public string SubObject { get; set; }

        [StringLength(9)]
        [DisplayFormat(NullDisplayText = "---------")]
        public string Project { get; set; }

        public bool IsDefault { get; set; }

        public bool IsActive { get; set; } = true;

        public Team Team { get; set; }

        public int TeamId { get; set; }
    }
}
