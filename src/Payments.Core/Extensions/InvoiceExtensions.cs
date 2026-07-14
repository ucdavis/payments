using System;
using Payments.Core.Domain;

namespace Payments.Core.Extensions
{
    public static class InvoiceExtensions
    {
        public static bool TryGetExternalReference(this Invoice invoice, out string url, out string label)
        {
            url = null;
            label = null;

            if (invoice == null ||
                !Uri.TryCreate(invoice.ExternalLink, UriKind.Absolute, out var externalReference) ||
                (externalReference.Scheme != Uri.UriSchemeHttp && externalReference.Scheme != Uri.UriSchemeHttps))
            {
                return false;
            }

            url = externalReference.AbsoluteUri;
            label = string.IsNullOrWhiteSpace(invoice.ExternalIdentifier)
                ? "Reference"
                : $"{invoice.ExternalIdentifier} Reference";

            return true;
        }
    }
}
