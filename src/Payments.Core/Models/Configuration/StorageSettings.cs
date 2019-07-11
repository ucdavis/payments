using System;

namespace Payments.Core.Models.Configuration
{
    public class StorageSettings
    {
        public string ConnectionString { get; set; }

        public string UrlBase { get; set; }

        public static string AttachmentContainerName = "attachments";
        public static string InvoicePdfContainerName = "invoice-pdfs";
        public static string ReceiptPdfContainerName = "receipt-pdfs";
    }
}
