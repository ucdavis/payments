using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Payments.Core.Domain;

namespace Payments.Mvc.Models.PaymentViewModels
{
    public class InvoicePaymentViewModel
    {
        public string CustomerName { get; set; }

        public string CustomerAddress { get; set; }

        public string CustomerEmail { get; set; }

        public decimal Discount { get; set; }

        public decimal TaxPercent { get; set; }

        public string Status { get; set; }

        public List<LineItem> Items { get; set; }

        public decimal Subtotal { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Total { get; set; }

        public Dictionary<string, string> PaymentDictionary { get; set; }
    }
}
