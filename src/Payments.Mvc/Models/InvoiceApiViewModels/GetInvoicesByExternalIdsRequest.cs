namespace Payments.Mvc.Models.InvoiceApiViewModels
{
    public class GetInvoicesByExternalIdsRequest
    {
        public string ExternalIdentifier { get; set; }

        public string[] ExternalIds { get; set; }
    }
}
