using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Payments.Core.Models.Invoice;
using Payments.Mvc.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Payments.Tests.Swagger
{
    public class CreateInvoiceSchemaFilterTests
    {
        [Fact]
        public void GenerateSchema_CreateInvoice_UsesFocusedDocumentation()
        {
            var services = new ServiceCollection();
            services.AddControllers();
            services.AddSwaggerGen(options => options.SchemaFilter<CreateInvoiceSchemaFilter>());
            using var provider = services.BuildServiceProvider();

            var generator = provider.GetRequiredService<ISchemaGenerator>();
            var repository = new SchemaRepository();

            generator.GenerateSchema(typeof(CreateInvoiceModel), repository);

            var invoiceSchema = repository.Schemas[nameof(CreateInvoiceModel)];
            var type = FindProperty(invoiceSchema, nameof(CreateInvoiceModel.Type));
            var useDefaultAccount = FindProperty(invoiceSchema, nameof(CreateInvoiceModel.UseDefaultAccount));
            var rechargeAccounts = FindProperty(invoiceSchema, nameof(CreateInvoiceModel.RechargeAccounts));
            var rechargeAccountSchema = repository.Schemas[nameof(CreateInvoiceRechargeAccountRequest)];

            Assert.Equal(new[] { "CC", "Recharge" },
                type.Enum.Cast<OpenApiString>().Select(value => value.Value));
            Assert.Contains("only if accountId does not identify an account", useDefaultAccount.Description);
            Assert.Equal(nameof(CreateInvoiceRechargeAccountRequest), rechargeAccounts.Items.Reference.Id);
            Assert.Equal(
                new[] { "amount", "direction", "financialSegmentString", "notes", "percentage" },
                rechargeAccountSchema.Properties.Keys.OrderBy(key => key, StringComparer.Ordinal));
        }

        private static Microsoft.OpenApi.Models.OpenApiSchema FindProperty(
            Microsoft.OpenApi.Models.OpenApiSchema schema, string propertyName)
        {
            return schema.Properties
                .Single(property => string.Equals(property.Key, propertyName, StringComparison.OrdinalIgnoreCase))
                .Value;
        }
    }
}
