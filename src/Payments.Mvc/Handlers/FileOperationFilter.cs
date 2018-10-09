using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Payments.Mvc.Handlers
{
    public class FileOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ParameterDescriptions.All(x => x.ModelMetadata.ContainerType != typeof(IFormFile)))
            {
                return;
            }

            operation.Parameters.Clear();
            operation.Parameters.Add(new NonBodyParameter
            {
                Name        = "file", // must match parameter name from controller method
                In          = "formData",
                Description = "Upload file.",
                Required    = true,
                Type        = "file"
            });
            operation.Consumes.Add("application/form-data");
        }
    }
}
