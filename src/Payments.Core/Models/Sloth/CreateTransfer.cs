using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Payments.Core.Models.Sloth
{
    public class CreateTransfer
    {
        /// <summary>
        /// Dollar amount associated with the transaction
        /// </summary>
        [Range(typeof(decimal), "0.01", "1000000000")]
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// Chart Code associated with transaction.
        /// </summary>
        [MaxLength(1)]
        public string Chart { get; set; }

        /// <summary>
        /// Account used in the general ledger to post transactions.
        /// Accounts are specific to a Chart Code.
        /// </summary>
        [MaxLength(7)]
        [RegularExpression("[A-Z0-9]*")]
        public string Account { get; set; }

        /// <summary>
        /// Sub-Account is an optional accounting unit attribute.
        /// Chart Code and Account are part of Sub-Account key.
        /// </summary>
        [MaxLength(5)]
        [RegularExpression("[A-Z0-9]*")]
        public string SubAccount { get; set; }

        /// <summary>
        /// Object codes represent all income, expense, asset, liability and fund balance classification
        ///  that are assigned to transactions and help identify the nature of the transaction.
        /// Object Codes are specific to a Chart Code.
        /// </summary>
        [MaxLength(4)]
        [RegularExpression("[A-Z0-9]*")]
        public string ObjectCode { get; set; }

        /// <summary>
        /// Sub-Object is an optional accounting unit attribute that allows finer 
        ///  distinctions within a particular object code on an account.
        /// Sub-Object codes are specific to a Chart Code, Account and Object Code combination.
        /// </summary>
        [MaxLength(3)]
        [RegularExpression("[A-Z0-9]*")]
        public string SubObjectCode { get; set; }

        /// <summary>
        /// Object Type defines the general use of an object code; such as income, asset, expense, or liability.
        /// Not a required field as the General Ledger will derives the value from the Object Code.
        /// It is recommended not to include these values.
        /// </summary>
        [MaxLength(2)]
        public string ObjectType { get; set; }

        /// <summary>
        /// A brief description of the specific transaction. Displays in reporting.
        /// PCI, HIPPA, FERPA and PII information is prohibited.
        /// </summary>
        [MaxLength(40)]
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Debit or Credit Code associated with the transaction.
        /// </summary>
        [Required]
        public Transfer.CreditDebit Direction { get; set; }

        public string FinancialSegmentString { get; set; }
    }
}
