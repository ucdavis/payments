using Payments.Core.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Payments.Core.Models.Invoice
{
    public class EditInvoiceModel
    {
        public int AccountId { get; set; }

        public EditInvoiceCustomerModel Customer { get; set; }

        public string Memo { get; set; }

        /// <summary>
        /// Coupon to be applied to order
        /// </summary>
        public int? CouponId { get; set; }

        [Range(0, int.MaxValue)]
        public decimal ManualDiscount { get; set; }

        [Range(0, int.MaxValue)]
        public decimal TaxPercent { get; set; }

        public IList<EditInvoiceItemModel> Items { get; set; }

        public IList<EditInvoiceAttachmentModel> Attachments { get; set; }

        public IList<RechargeAccount> RechargeAccounts { get; set; } = new List<RechargeAccount>();

        public DateTime? DueDate { get; set; }
    }

    public class EditInvoiceCustomerModel
    {
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Address { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(100)]
        public string Company { get; set; }
    }

    public class EditInvoiceItemModel
    {
        [MaxLength(500)]
        public string Description { get; set; }

        [Range(0, 1_000_000)]
        public decimal Quantity { get; set; }

        [Range(0, 1_000_000)]
        public decimal Amount { get; set; }

        public bool TaxExempt { get; set; }

        [Range(0, 1_000_000)]
        public decimal Total { get; set; }
    }

    public class EditInvoiceAttachmentModel
    {
        /// <summary>
        /// Identifier returned from upload api
        /// </summary>
        public string Identifier { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public long Size { get; set; }
    }
}
