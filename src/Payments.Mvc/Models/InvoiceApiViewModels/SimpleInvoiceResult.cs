namespace Payments.Mvc.Models.InvoiceApiViewModels
{
    public class SimpleInvoiceResult
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public string Type { get; set; }

        public string LinkId { get; set; }

        public string CustomerEmail { get; set; }

        public string TotalAmount { get; set; }
    }
}
