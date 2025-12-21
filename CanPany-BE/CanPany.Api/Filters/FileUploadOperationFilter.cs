using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace CanPany.Api.Filters;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if any parameter is IFormFile
        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) || 
                       p.ParameterType == typeof(IFormFile[]) ||
                       p.ParameterType == typeof(IFormFileCollection))
            .ToList();

        if (fileParams.Any())
        {
            // Remove all parameters that are IFormFile
            if (operation.Parameters != null)
            {
                operation.Parameters = operation.Parameters
                    .Where(p => !fileParams.Any(fp => fp.Name == p.Name))
                    .ToList();
            }
            
            // Create multipart/form-data request body
            var properties = new Dictionary<string, OpenApiSchema>();
            var required = new HashSet<string>();

            foreach (var param in fileParams)
            {
                var paramName = param.Name ?? "file";
                var isArray = param.ParameterType == typeof(IFormFile[]) || 
                             param.ParameterType == typeof(IFormFileCollection);
                
                properties[paramName] = new OpenApiSchema
                {
                    Type = isArray ? "array" : "string",
                    Format = isArray ? null : "binary",
                    Description = $"Upload {paramName}",
                    Items = isArray ? new OpenApiSchema { Type = "string", Format = "binary" } : null
                };
                
                if (!param.HasDefaultValue && !param.IsOptional)
                {
                    required.Add(paramName);
                }
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = required.Any(),
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = properties,
                            Required = required
                        }
                    }
                }
            };
        }
    }
}

