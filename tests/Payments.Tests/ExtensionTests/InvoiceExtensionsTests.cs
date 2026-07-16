using Payments.Core.Domain;
using Payments.Core.Extensions;
using Xunit;

namespace payments.Tests.ExtensionTests
{
    public class InvoiceExtensionsTests
    {
        [Theory]
        [InlineData("https://example.com/records/record-123")]
        [InlineData("http://example.com/records/record-123")]
        public void TryGetExternalReference_HttpLink_ReturnsReference(string externalLink)
        {
            var invoice = new Invoice
            {
                ExternalId = "record-123",
                ExternalIdentifier = "External System",
                ExternalLink = externalLink,
            };

            var result = invoice.TryGetExternalReference(out var url, out var label);

            Assert.True(result);
            Assert.Equal(externalLink, url);
            Assert.Equal("External System Reference", label);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("not a URL")]
        [InlineData("/records/record-123")]
        [InlineData("javascript:alert('hello')")]
        [InlineData("ftp://example.com/records/record-123")]
        public void TryGetExternalReference_InvalidOrUnsupportedLink_ReturnsFalse(string externalLink)
        {
            var invoice = new Invoice
            {
                ExternalId = "record-123",
                ExternalLink = externalLink,
            };

            var result = invoice.TryGetExternalReference(out var url, out var label);

            Assert.False(result);
            Assert.Null(url);
            Assert.Null(label);
        }

        [Fact]
        public void TryGetExternalReference_MissingExternalIdentifier_UsesGenericLabel()
        {
            var invoice = new Invoice
            {
                ExternalId = "record-123",
                ExternalLink = "https://example.com/records/record-123",
            };

            var result = invoice.TryGetExternalReference(out _, out var label);

            Assert.True(result);
            Assert.Equal("Reference", label);
        }
    }
}
