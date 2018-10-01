using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Payments.Core.Models.Sloth
{
    public class Transaction
    {
        public Transaction()
        {
            Transfers = new List<Transfer>();
        }

        public string Id { get; set; }

        public string CreatorName { get; set; }

        public string Status { get; set; }

        public string SourceName { get; set; }

        public string SourceType { get; set; }

        /// <summary>
        /// Tracking Number created by the merchant accountant
        /// </summary>
        [DisplayName("Merchant Tracking Number")]
        public string MerchantTrackingNumber { get; set; }

        /// <summary>
        /// Tracking Number created by the payment processor
        /// </summary>
        [DisplayName("Processor Tracking Number")]
        public string ProcessorTrackingNumber { get; set; }

        /// <summary>
        /// Unique feed origination identifier given to the Feed System.
        /// The origination code is validated in during file receipt and in the processing.
        /// </summary>
        public string OriginCode { get; set; }

        /// <summary>
        /// Unique identifier for a set of related transactions per origination code.
        /// A file can have multiple document numbers but the file must balance by document number(aka net zero) and by total amount.Debits = Credits
        /// Once a document number posts to the general ledger then it cannot be used again.
        /// </summary>
        [MinLength(1)]
        [MaxLength(14)]
        [RegularExpression("[A-Z0-9]*")]
        [Required]
        [DisplayName("Document Number")]
        public string DocumentNumber { get; set; }

        /// <summary>
        /// Financial System document type associated with the feed.
        /// Feed systems will be authorized to use a specific value based on transactions.
        /// </summary>
        [NotMapped]
        public string DocumentType { get; set; }

        /// <summary>
        /// Primarily used in Decision Support reporting for additional transaction identification.
        /// Equivalent to the KFS Organization Document Number.
        /// </summary>
        [MinLength(1)]
        [MaxLength(10)]
        [DisplayName("Kfs Tracking Number")]
        public string KfsTrackingNumber { get; set; }

        /// <summary>
        /// Date the transaction occurred.
        /// </summary>
        [Required]
        [DisplayName("Transaction Date")]
        public DateTime TransactionDate { get; set; }

        public IList<Transfer> Transfers { get; set; }
    }
}
