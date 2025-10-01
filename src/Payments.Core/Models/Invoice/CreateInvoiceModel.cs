using Payments.Core.Domain;
using Payments.Core.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Payments.Core.Models.Invoice
{
    public class CreateInvoiceModel
    {
        public int AccountId { get; set; }

        /// <summary>
        /// One or many customers to assign an invoice to. Multiple customers will create multiple invoices
        /// </summary>
        public IList<CreateInvoiceCustomerModel> Customers { get; set; }

        /// <summary>
        /// Coupon to be applied to order
        /// </summary>
        public int? CouponId { get; set; }

        /// <summary>
        /// Dollar amount discount to be applied to entire order
        /// </summary>
        [Range(0, int.MaxValue)]
        public decimal ManualDiscount { get; set; }

        /// <summary>
        /// Tax rate to be applied to entire order
        /// </summary>
        [Range(0, int.MaxValue)]
        public decimal TaxPercent { get; set; }

        /// <summary>
        /// Extra text to be displayed to customer
        /// </summary>
        public string Memo { get; set; }

        /// <summary>
        /// Line items
        /// </summary>
        public IList<CreateInvoiceItemModel> Items { get; set; }

        /// <summary>
        /// Attachments for invoice. Pre upload and specify returned identifiers 
        /// </summary>
        public IList<CreateInvoiceAttachmentModel> Attachments { get; set; }

        public IList<RechargeAccount> RechargeAccounts { get; set; } = new List<RechargeAccount>();

        /// <summary>
        /// Invoice type - must be either "CC" (Credit Card) or "Recharge"
        /// </summary>
        [Required]
        [ValidInvoiceType]
        public string Type { get; set; } //New invoice type

        /// <summary>
        /// Optional due date for invoice
        /// </summary>
        public DateTime? DueDate { get; set; }
    }

    public class CreateInvoiceCustomerModel
    {
        /// <summary>
        /// Customer name
        /// </summary>
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Customer address
        /// </summary>
        [MaxLength(100)]
        public string Address { get; set; }

        /// <summary>
        /// Customer email address. Required
        /// </summary>
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// costumer company
        /// </summary>
        [MaxLength(100)]
        public string Company { get; set; }
    }

    public class CreateInvoiceItemModel
    {
        /// <summary>
        /// Name of invoice item
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Number of invoice items
        /// </summary>
        [Range(0, 1_000_000)]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Price per item
        /// </summary>
        [Range(0, 1_000_000)]
        public decimal Amount { get; set; }

        public bool TaxExempt { get; set; }
    }

    public class CreateInvoiceAttachmentModel
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
