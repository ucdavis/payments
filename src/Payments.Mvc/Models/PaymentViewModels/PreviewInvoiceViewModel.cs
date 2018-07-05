using System;
using System.Collections.Generic;
using System.Linq;
using Payments.Core.Domain;

namespace Payments.Mvc.Models.PaymentViewModels
{
    public class PreviewInvoiceViewModel
    {
        public string Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerAddress { get; set; }
        public string Memo { get; set; }
        public List<LineItem> Items { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxPercent { get; set; }
        public string Status { get; set; }
        public string TeamName { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime PaidDate { get; set; }

        public void UpdateCalculatedValues()
        {
            Subtotal = Items.Sum(i => i.Total);
            TaxAmount = (Subtotal - Discount) * TaxPercent;
            Total = Subtotal - Discount + TaxAmount;
        }
    }
}
