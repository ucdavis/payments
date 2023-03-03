using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Payments.Core.Models.Sloth
{
    public class CreateTransaction
    {
        public CreateTransaction()
        {
            AutoApprove = false;
            Transfers = new List<CreateTransfer>();
        }

        /// <summary>
        /// Auto approve this transaction for upload to KFS
        /// </summary>
        public bool AutoApprove { get; set; }

        /// <summary>
        /// Tracking Number created by the merchant accountant
        /// </summary>
        public string MerchantTrackingNumber { get; set; }

        /// <summary>
        ///  URL created by the merchant accountant referring to originating action
        /// </summary>
        public string MerchantTrackingUrl { get; set; }

        /// <summary>
        /// Tracking Number created by the payment processor
        /// </summary>
        public string ProcessorTrackingNumber { get; set; }

        /// <summary>
        /// Optionally set the kfs tracking number to be used
        /// </summary>
        [MaxLength(10)]
        public string KfsTrackingNumber { get; set; }

        /// <summary>
        /// Date the transaction occurred.
        /// </summary>
        [Required]
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Source of the transactions
        /// e.g. CyberSource
        /// </summary>
        [Required]
        public string Source { get; set; }

        /// <summary>
        /// Type of the transactions by their source
        /// e.g. Income
        /// </summary>
        [Required]
        public string SourceType { get; set; }

        public string Description { get; set; }

        public bool ValidateFinancialSegmentStrings { get; set; } = false; //Don't have sloth reject if the COA isn't valid.

        public IList<MetadataEntry> Metadata { get; set; } = new List<MetadataEntry>();

        public void AddMetadata(string name, string value)
        {
            Metadata.Add(new MetadataEntry { Name = name, Value = value });
        }

        public class MetadataEntry
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        [Required]
        public IList<CreateTransfer> Transfers { get; set; }
    }
}
