using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Payments.Core.Domain;
using Payments.Mvc.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Payments.Tests.Swagger
{
    public class InvoiceSchemaFilterTests
    {
        [Fact]
        public void GenerateSchema_Invoice_MatchesSerializedResponse()
        {
            var services = new ServiceCollection();
            services.AddControllers();
            services.AddSwaggerGen(options => options.SchemaFilter<InvoiceSchemaFilter>());
            using var provider = services.BuildServiceProvider();

            var generator = provider.GetRequiredService<ISchemaGenerator>();
            var repository = new SchemaRepository();

            generator.GenerateSchema(typeof(Invoice), repository);

            var invoiceSchema = repository.Schemas[nameof(Invoice)];

            AssertMissing(invoiceSchema, nameof(Invoice.Attachments));
            AssertMissing(invoiceSchema, nameof(Invoice.Team));
            AssertMissing(invoiceSchema, nameof(Invoice.History));
            AssertMissing(invoiceSchema, nameof(Invoice.PaymentEvents));

            AssertNullExample(invoiceSchema, nameof(Invoice.Coupon));
            AssertNullExample(invoiceSchema, nameof(Invoice.Account));

            AssertMissing(repository.Schemas[nameof(Coupon)], nameof(Coupon.Team));
            AssertMissing(repository.Schemas[nameof(Coupon)], nameof(Coupon.Invoices));
            AssertMissing(repository.Schemas[nameof(FinancialAccount)], nameof(FinancialAccount.Team));
            AssertMissing(repository.Schemas[nameof(RechargeAccount)], nameof(RechargeAccount.Invoice));

            var teamName = invoiceSchema.Properties
                .Single(property => string.Equals(
                    property.Key, nameof(Invoice.TeamName), StringComparison.OrdinalIgnoreCase))
                .Value;
            Assert.Contains("full team object is not returned", teamName.Description);
        }

        private static void AssertMissing(Microsoft.OpenApi.Models.OpenApiSchema schema, string propertyName)
        {
            Assert.DoesNotContain(schema.Properties.Keys,
                key => string.Equals(key, propertyName, StringComparison.OrdinalIgnoreCase));
        }

        private static void AssertNullExample(Microsoft.OpenApi.Models.OpenApiSchema schema, string propertyName)
        {
            var property = schema.Properties
                .Single(item => string.Equals(item.Key, propertyName, StringComparison.OrdinalIgnoreCase))
                .Value;

            Assert.Null(property.Reference);
            Assert.True(property.Nullable);
            Assert.IsType<OpenApiNull>(property.Example);
            Assert.Equal("Not populated by this endpoint.", property.Description);
        }
    }
}
