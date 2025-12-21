using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace CanPany.Api.Middlewares;

/// <summary>
/// Global Exception Handler Middleware - Đảm bảo không có exception nào làm hệ thống dừng
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Unhandled exception occurred. Path: {Path}, Method: {Method}",
                context.Request.Path, context.Request.Method);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        // Get localization service
        var localizationService = context.RequestServices.GetService<ILocalizationService>();
        var language = localizationService?.GetCurrentLanguage() ?? "en";

        var errorResponse = new ErrorResponse
        {
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case EntityNotFoundException ex:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Status = 404;
                errorResponse.Title = localizationService?.GetError("UserNotFound", language) ?? "Resource Not Found";
                errorResponse.Detail = localizationService?.GetError("UserNotFound", language) ?? ex.Message;
                errorResponse.Type = "https://httpstatuses.com/404";
                break;

            case BusinessRuleViolationException ex:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Status = 400;
                errorResponse.Title = localizationService?.GetError("ValidationFailed", language) ?? "Business Rule Violation";
                errorResponse.Detail = localizationService?.GetString(ex.RuleName, language) ?? ex.Message;
                errorResponse.Type = "https://httpstatuses.com/400";
                break;

            case InvalidDomainOperationException ex:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Status = 400;
                errorResponse.Title = "Invalid Operation";
                errorResponse.Detail = ex.Message;
                errorResponse.Type = "https://httpstatuses.com/400";
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Status = 401;
                errorResponse.Title = localizationService?.GetError("Unauthorized", language) ?? "Unauthorized";
                errorResponse.Detail = localizationService?.GetError("Unauthorized", language) ?? "You are not authorized to perform this action";
                errorResponse.Type = "https://httpstatuses.com/401";
                break;

            case ArgumentNullException:
            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Status = 400;
                errorResponse.Title = "Invalid Argument";
                errorResponse.Detail = exception.Message;
                errorResponse.Type = "https://httpstatuses.com/400";
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Status = 500;
                errorResponse.Title = localizationService?.GetError("InternalServer", language) ?? "Internal Server Error";
                errorResponse.Detail = localizationService?.GetError("InternalServer", language) ?? "An unexpected error occurred. Please try again later.";
                errorResponse.Type = "https://httpstatuses.com/500";
                
                // Log full exception details for internal errors
                var logger = context.RequestServices.GetRequiredService<ILogger<GlobalExceptionHandlerMiddleware>>();
                logger.LogError(exception, 
                    "Unhandled exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                    exception.GetType().FullName, exception.Message, exception.StackTrace);
                break;
        }

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

