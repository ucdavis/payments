namespace Payments.Mvc.Models.InvoiceApiViewModels
{
    public class SimpleInvoiceResult
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public string Type { get; set; }

        public string LinkId { get; set; }

        public string CustomerEmail { get; set; }

        public decimal TotalAmount { get; set; }

        public string ExternalIdentifier { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public string ExternalLink { get; set; } = string.Empty;    
    }
}
