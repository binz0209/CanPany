using CanPany.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace CanPany.Api.Filters;

/// <summary>
/// API Exception Filter - Xử lý exceptions từ controllers
/// </summary>
public class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        var result = new ObjectResult(null)
        {
            StatusCode = (int)HttpStatusCode.InternalServerError
        };

        switch (exception)
        {
            case EntityNotFoundException ex:
                result.StatusCode = (int)HttpStatusCode.NotFound;
                result.Value = new
                {
                    type = "https://httpstatuses.com/404",
                    title = "Resource Not Found",
                    status = 404,
                    detail = ex.Message,
                    traceId = context.HttpContext.TraceIdentifier
                };
                _logger.LogWarning(ex, "Entity not found: {Message}", ex.Message);
                break;

            case BusinessRuleViolationException ex:
                result.StatusCode = (int)HttpStatusCode.BadRequest;
                result.Value = new
                {
                    type = "https://httpstatuses.com/400",
                    title = "Business Rule Violation",
                    status = 400,
                    detail = ex.Message,
                    traceId = context.HttpContext.TraceIdentifier
                };
                _logger.LogWarning(ex, "Business rule violation: {Message}", ex.Message);
                break;

            case InvalidDomainOperationException ex:
                result.StatusCode = (int)HttpStatusCode.BadRequest;
                result.Value = new
                {
                    type = "https://httpstatuses.com/400",
                    title = "Invalid Operation",
                    status = 400,
                    detail = ex.Message,
                    traceId = context.HttpContext.TraceIdentifier
                };
                _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
                break;

            case UnauthorizedAccessException:
                result.StatusCode = (int)HttpStatusCode.Unauthorized;
                result.Value = new
                {
                    type = "https://httpstatuses.com/401",
                    title = "Unauthorized",
                    status = 401,
                    detail = "You are not authorized to perform this action",
                    traceId = context.HttpContext.TraceIdentifier
                };
                _logger.LogWarning("Unauthorized access attempt");
                break;

            case ArgumentNullException:
            case ArgumentException:
                result.StatusCode = (int)HttpStatusCode.BadRequest;
                result.Value = new
                {
                    type = "https://httpstatuses.com/400",
                    title = "Invalid Argument",
                    status = 400,
                    detail = exception.Message,
                    traceId = context.HttpContext.TraceIdentifier
                };
                _logger.LogWarning(exception, "Invalid argument: {Message}", exception.Message);
                break;

            default:
                result.Value = new
                {
                    type = "https://httpstatuses.com/500",
                    title = "Internal Server Error",
                    status = 500,
                    detail = "An unexpected error occurred. Please try again later.",
                    traceId = context.HttpContext.TraceIdentifier
                };
                _logger.LogError(exception, 
                    "Unhandled exception in controller: {Controller}, Action: {Action}",
                    context.RouteData.Values["controller"],
                    context.RouteData.Values["action"]);
                break;
        }

        context.Result = result;
        context.ExceptionHandled = true;
    }
}
