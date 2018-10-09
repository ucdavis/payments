using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Payments.Mvc.Models.InvoiceViewModels
{
    public class CreateInvoiceViewModel
    {
        public int AccountId { get; set; }

        /// <summary>
        /// One or many customers to assign an invoice to. Multiple customers will create multiple invoices
        /// </summary>
        public IList<CreateInvoiceCustomerViewModel> Customers { get; set; }

        /// <summary>
        /// Dollar amount discount to be applied to entire order
        /// </summary>
        [Range(0, int.MaxValue)]
        public decimal Discount { get; set; }

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
        public IList<CreateInvoiceItemViewModel> Items { get; set; }

        /// <summary>
        /// Attachments for invoice. Pre upload and specify returned identifiers 
        /// </summary>
        public IList<CreateInvoiceAttachmentViewModel> Attachments { get; set; }

        /// <summary>
        /// Optional due date for invoice
        /// </summary>
        public DateTime? DueDate { get; set; }
    }

    public class CreateInvoiceCustomerViewModel
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
    }

    public class CreateInvoiceItemViewModel
    {
        /// <summary>
        /// Name of invoice item
        /// </summary>
        [MaxLength(100)]
        public string Description { get; set; }

        /// <summary>
        /// Number of invoice items
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        /// <summary>
        /// Price per item
        /// </summary>
        [Range(0.01, int.MaxValue)]
        public decimal Amount { get; set; }
    }

    public class CreateInvoiceAttachmentViewModel
    {
        public string Identifier { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public int Size { get; set; }
    }
}
