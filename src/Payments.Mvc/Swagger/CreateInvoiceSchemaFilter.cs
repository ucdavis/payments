using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Payments.Core.Models.Invoice;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Payments.Mvc.Swagger
{
    public class CreateInvoiceSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type != typeof(CreateInvoiceModel))
            {
                return;
            }

            Describe(schema, nameof(CreateInvoiceModel.AccountId),
                "Financial account identifier for the team. Required for CC invoices unless useDefaultAccount finds the team's active default account.");
            Describe(schema, nameof(CreateInvoiceModel.Customers),
                "Customers to invoice. A separate invoice is created for each customer.");
            Describe(schema, nameof(CreateInvoiceModel.CouponId),
                "Optional coupon identifier. Coupons are not allowed for Recharge invoices.");
            Describe(schema, nameof(CreateInvoiceModel.ManualDiscount),
                "Dollar discount applied to the entire invoice.");
            Describe(schema, nameof(CreateInvoiceModel.TaxPercent),
                "Tax percentage applied to the entire invoice. Tax is not allowed for Recharge invoices.");
            Describe(schema, nameof(CreateInvoiceModel.Memo), "Additional text displayed to the customer.");
            Describe(schema, nameof(CreateInvoiceModel.Items), "Invoice line items.");
            Describe(schema, nameof(CreateInvoiceModel.Attachments),
                "Previously uploaded invoice attachments, identified by the value returned from the upload API.");
            Describe(schema, nameof(CreateInvoiceModel.DueDate), "Optional invoice due date.");

            var useDefaultAccount = FindProperty(schema, nameof(CreateInvoiceModel.UseDefaultAccount));
            if (useDefaultAccount != null)
            {
                useDefaultAccount.Description =
                    "When true, use the team's active default account only if accountId does not identify an account for the team. A valid supplied accountId is always used.";
                useDefaultAccount.Default = new OpenApiBoolean(false);
            }

            var type = FindProperty(schema, nameof(CreateInvoiceModel.Type));
            if (type != null)
            {
                type.Description = "Invoice type: CC for a credit-card invoice, or Recharge for a recharge invoice.";
                type.Enum = StringEnum(
                    Payments.Core.Domain.Invoice.InvoiceTypes.CreditCard,
                    Payments.Core.Domain.Invoice.InvoiceTypes.Recharge);
                type.Default = new OpenApiString(Payments.Core.Domain.Invoice.InvoiceTypes.CreditCard);
            }

            var rechargeAccounts = FindProperty(schema, nameof(CreateInvoiceModel.RechargeAccounts));
            if (rechargeAccounts != null)
            {
                rechargeAccounts.Description =
                    "Financial segments used by a Recharge invoice, or a single Credit entry used as the account override for a CC invoice.";
                rechargeAccounts.Items = context.SchemaGenerator.GenerateSchema(
                    typeof(CreateInvoiceRechargeAccountRequest), context.SchemaRepository);
                ConfigureDirection(context.SchemaRepository);
            }
        }

        private static void ConfigureDirection(SchemaRepository repository)
        {
            var requestSchema = repository.Schemas
                .FirstOrDefault(s => s.Key.EndsWith(nameof(CreateInvoiceRechargeAccountRequest), StringComparison.Ordinal))
                .Value;
            var direction = FindProperty(requestSchema, nameof(CreateInvoiceRechargeAccountRequest.Direction));

            if (direction != null)
            {
                direction.Enum = StringEnum("Credit", "Debit");
                direction.Default = new OpenApiString("Credit");
            }
        }

        private static IList<IOpenApiAny> StringEnum(params string[] values)
        {
            return values.Select(value => (IOpenApiAny)new OpenApiString(value)).ToList();
        }

        private static void Describe(OpenApiSchema schema, string propertyName, string description)
        {
            var property = FindProperty(schema, propertyName);
            if (property != null)
            {
                property.Description = description;
            }
        }

        private static OpenApiSchema FindProperty(OpenApiSchema schema, string propertyName)
        {
            return schema?.Properties?
                .FirstOrDefault(p => string.Equals(p.Key, propertyName, StringComparison.OrdinalIgnoreCase))
                .Value;
        }
    }

    /// <summary>
    /// Recharge account fields accepted when creating an invoice.
    /// </summary>
    public class CreateInvoiceRechargeAccountRequest
    {
        /// <summary>Credit adds money to the financial segment; Debit removes money from it.</summary>
        [Required]
        public string Direction { get; set; } = "Credit";

        /// <summary>Aggie Enterprise financial segment string.</summary>
        [Required]
        [StringLength(128)]
        public string FinancialSegmentString { get; set; }

        /// <summary>Dollar amount allocated to this financial segment.</summary>
        [Range(0.01, 1_000_000_000)]
        public decimal Amount { get; set; }

        /// <summary>Percentage of the invoice total allocated to this financial segment.</summary>
        public decimal Percentage { get; set; }

        /// <summary>Optional notes for this financial segment.</summary>
        [StringLength(400)]
        public string Notes { get; set; }
    }
}
