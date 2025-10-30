using System;
using System.Collections.Generic;
using Payments.Core.Domain;

namespace Payments.Mvc.Models.PaymentViewModels
{
    public class RechargeInvoiceViewModel
    {
        public RechargeInvoiceViewModel()
        {
            Items = new List<LineItem>();
            Attachments = new List<InvoiceAttachment>();
            DebitRechargeAccounts = new List<RechargeAccount>();
        }

        public string Id { get; set; }
        public string LinkId { get; set; }

        //public string CustomerName { get; set; }

        //public string CustomerCompany { get; set; }

        //public string CustomerAddress { get; set; }

        public string CustomerEmail { get; set; }

        public string Memo { get; set; }

        //public decimal Discount { get; set; }

        //public decimal TaxPercent { get; set; }

        public IList<LineItem> Items { get; set; }

        public IList<InvoiceAttachment> Attachments { get; set; }

        //public Coupon Coupon { get; set; }

        public decimal Subtotal { get; set; }

        //public decimal TaxAmount { get; set; }

        public decimal Total { get; set; }

        public PaymentInvoiceTeamViewModel Team { get; set; }

        public DateTime? DueDate { get; set; }

        public bool Paid { get; set; }

        public DateTime? PaidDate { get; set; }

        public string Status { get; set; }

        public IList<RechargeAccount> DebitRechargeAccounts { get; set; } //Might want to change the name of this to RechargeAccounts.
    }
}
