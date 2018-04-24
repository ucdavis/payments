﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Payments.Mvc.Models.InvoiceViewModels
{
    public class EditInvoiceViewModel
    {
        [MaxLength(100)]
        public string CustomerName { get; set; }

        [MaxLength(100)]
        public string CustomerAddress { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string CustomerEmail { get; set; }

        [Range(0, int.MaxValue)]
        public decimal Discount { get; set; }

        [Range(0, int.MaxValue)]
        public decimal TaxPercent { get; set; }

        public IList<EditInvoiceItemViewModel> Items { get; set; }
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