﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Payments.Mvc.Models.InvoiceViewModels
{
    public class EditInvoiceViewModel
    {
        public int AccountId { get; set; }

        public EditInvoiceCustomerViewModel Customer { get; set; }

        public string Memo { get; set; }

        /// <summary>
        /// Coupon to be applied to order
        /// </summary>
        public int CouponId { get; set; }

        [Range(0, int.MaxValue)]
        public decimal Discount { get; set; }

        [Range(0, int.MaxValue)]
        public decimal TaxPercent { get; set; }

        public IList<EditInvoiceItemViewModel> Items { get; set; }

        public IList<EditInvoiceAttachmentViewModel> Attachments { get; set; }

        public DateTime? DueDate { get; set; }
    }

    public class EditInvoiceCustomerViewModel
    {
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Address { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class EditInvoiceItemViewModel
    {
        [MaxLength(100)]
        public string Description { get; set; }

        [Range(0, 1_000_000)]
        public decimal Quantity { get; set; }

        [Range(0, 1_000_000)]
        public decimal Amount { get; set; }

        public bool TaxExempt { get; set; }

        [Range(0, 1_000_000)]
        public decimal Total { get; set; }
    }

    public class EditInvoiceAttachmentViewModel
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
