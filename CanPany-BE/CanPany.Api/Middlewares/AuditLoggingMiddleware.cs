using CanPany.Application.Interfaces.Services;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;

namespace CanPany.Api.Middlewares;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        // Bỏ qua OPTIONS requests (CORS preflight) - không cần audit
        if (context.Request.Method == "OPTIONS")
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var startTime = DateTime.UtcNow;

        // Get user info
        var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = context.User?.FindFirstValue(ClaimTypes.Email);

        // Get request info
        var requestPath = context.Request.Path.Value ?? string.Empty;
        var queryString = context.Request.QueryString.Value;
        var httpMethod = context.Request.Method;
        var endpoint = $"{httpMethod} {requestPath}";
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers["User-Agent"].ToString();

        // Read request body
        string? requestBody = null;
        if (context.Request.ContentLength > 0 && 
            context.Request.ContentType?.Contains("application/json") == true)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        // Log request
        try
        {
            await auditService.LogRequestAsync(
                userId,
                userEmail,
                httpMethod,
                endpoint,
                requestPath,
                queryString,
                requestBody,
                ipAddress,
                userAgent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging request audit");
        }

        // Capture response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log error
            try
            {
                await auditService.LogErrorAsync(
                    userId,
                    endpoint,
                    ex.Message,
                    ex.StackTrace,
                    context.Response.StatusCode);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Error logging error audit");
            }
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;

            // Read response body
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);

            // Log response
            try
            {
                await auditService.LogResponseAsync(
                    userId,
                    endpoint,
                    context.Response.StatusCode,
                    responseBodyText.Length > 10000 ? "[Response too large]" : responseBodyText,
                    duration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging response audit");
            }

            // Copy response back to original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}

