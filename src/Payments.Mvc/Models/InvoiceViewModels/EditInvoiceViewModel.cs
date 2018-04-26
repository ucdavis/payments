using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Payments.Mvc.Models.InvoiceViewModels
{
    public class EditInvoiceViewModel
    {
        public EditInvoiceCustomerViewModel Customer { get; set; }

        [Range(0, int.MaxValue)]
        public decimal Discount { get; set; }

        [Range(0, int.MaxValue)]
        public decimal Tax { get; set; }

        public IList<EditInvoiceItemViewModel> Items { get; set; }
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

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0.01, int.MaxValue)]
        public decimal Amount { get; set; }
    }
}
