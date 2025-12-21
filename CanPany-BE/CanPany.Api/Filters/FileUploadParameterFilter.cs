using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace CanPany.Api.Filters;

public class FileUploadParameterFilter : IParameterFilter
{
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (context.ParameterInfo?.ParameterType == typeof(IFormFile) ||
            context.ParameterInfo?.ParameterType == typeof(IFormFile[]))
        {
            // Remove this parameter from the operation - it will be handled by RequestBody
            parameter.In = null;
            parameter.Schema = new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            };
        }
    }
}

