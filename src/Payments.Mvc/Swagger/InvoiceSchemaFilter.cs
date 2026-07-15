using System;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Payments.Core.Domain;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Payments.Mvc.Swagger
{
    public class InvoiceSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var ignoredProperties = context.Type.GetProperties()
                .Where(property => property.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Any());

            foreach (var ignoredProperty in ignoredProperties)
            {
                Remove(schema, ignoredProperty.Name);
            }

            if (context.Type != typeof(Invoice))
            {
                return;
            }

            ConfigureUnloadedNavigation(schema, nameof(Invoice.Coupon));
            ConfigureUnloadedNavigation(schema, nameof(Invoice.Account));

            var teamName = FindProperty(schema, nameof(Invoice.TeamName));
            if (teamName != null)
            {
                teamName.Description = "Name of the team that owns the invoice. The full team object is not returned.";
            }
        }

        private static void ConfigureUnloadedNavigation(OpenApiSchema schema, string propertyName)
        {
            var property = FindProperty(schema, propertyName);
            if (property == null)
            {
                return;
            }

            property.Reference = null;
            property.Type = "object";
            property.Nullable = true;
            property.Example = new OpenApiNull();
            property.Description = "Not populated by this endpoint.";
        }

        private static void Remove(OpenApiSchema schema, string propertyName)
        {
            var schemaPropertyName = schema?.Properties?.Keys
                .FirstOrDefault(p => string.Equals(p, propertyName, StringComparison.OrdinalIgnoreCase));

            if (schemaPropertyName != null)
            {
                schema.Properties.Remove(schemaPropertyName);
            }

            var requiredPropertyName = schema?.Required?
                .FirstOrDefault(p => string.Equals(p, propertyName, StringComparison.OrdinalIgnoreCase));

            if (requiredPropertyName != null)
            {
                schema.Required.Remove(requiredPropertyName);
            }
        }

        private static OpenApiSchema FindProperty(OpenApiSchema schema, string propertyName)
        {
            return schema?.Properties?
                .FirstOrDefault(p => string.Equals(p.Key, propertyName, StringComparison.OrdinalIgnoreCase))
                .Value;
        }
    }
}
