using CanPany.Application.Interfaces.Services;

namespace CanPany.Api.Middlewares;

/// <summary>
/// Middleware để detect language từ request header và set cho I18N service
/// </summary>
public class LanguageDetectionMiddleware
{
    private readonly RequestDelegate _next;

    public LanguageDetectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        // Get I18N service từ scope
        var i18nService = serviceProvider.GetService<II18nService>();
        
        if (i18nService != null)
        {
            // Detect language từ header "Accept-Language" hoặc query parameter "lang"
            var language = context.Request.Query["lang"].ToString();
            
            if (string.IsNullOrWhiteSpace(language))
            {
                // Try to get from Accept-Language header
                var acceptLanguage = context.Request.Headers["Accept-Language"].ToString();
                if (!string.IsNullOrWhiteSpace(acceptLanguage))
                {
                    // Parse Accept-Language header (e.g., "en-US,en;q=0.9,vi;q=0.8")
                    var languages = acceptLanguage.Split(',')
                        .Select(l => l.Split(';')[0].Trim().ToLower())
                        .ToList();

                    // Check for Vietnamese first, then English
                    if (languages.Any(l => l.StartsWith("vi")))
                        language = "vi";
                    else if (languages.Any(l => l.StartsWith("en")))
                        language = "en";
                }
            }

            // Default to English if not specified
            if (string.IsNullOrWhiteSpace(language))
                language = "en";

            // Set language cho I18N service
            i18nService.SetLanguage(language);
        }

        await _next(context);
    }
}




