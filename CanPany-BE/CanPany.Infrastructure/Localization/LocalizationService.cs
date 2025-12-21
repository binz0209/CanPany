using CanPany.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Resources;

namespace CanPany.Infrastructure.Localization;

/// <summary>
/// Implementation của ILocalizationService sử dụng Resource files
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<LocalizationService> _logger;
    private readonly ResourceManager _resourceManager;
    private const string DefaultLanguage = "en";
    private const string VietnameseLanguage = "vi";

    public LocalizationService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<LocalizationService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _resourceManager = new ResourceManager("CanPany.Infrastructure.Localization.Resources.Messages", typeof(LocalizationService).Assembly);
    }

    public string GetString(string key, string? language = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        language ??= GetCurrentLanguage();
        var culture = GetCultureInfo(language);

        try
        {
            var value = _resourceManager.GetString(key, culture);
            return value ?? key; // Trả về key nếu không tìm thấy
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get localized string for key: {Key}, language: {Language}", key, language);
            return key;
        }
    }

    public string GetString(string key, object?[] parameters, string? language = null)
    {
        var message = GetString(key, language);
        try
        {
            return string.Format(message, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to format localized string for key: {Key}", key);
            return message;
        }
    }

    public string GetError(string errorKey, string? language = null)
    {
        return GetString($"Error_{errorKey}", language);
    }

    public string GetSuccess(string successKey, string? language = null)
    {
        return GetString($"Success_{successKey}", language);
    }

    public string GetValidation(string validationKey, string? language = null)
    {
        return GetString($"Validation_{validationKey}", language);
    }

    public string GetStatus(string statusKey, string? language = null)
    {
        return GetString($"Status_{statusKey}", language);
    }

    public string GetCurrentLanguage()
    {
        try
        {
            // Lấy từ HTTP header "Accept-Language" hoặc query parameter "lang"
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                // Check query parameter first
                var langParam = httpContext.Request.Query["lang"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(langParam))
                {
                    return NormalizeLanguage(langParam);
                }

                // Check header
                var acceptLanguage = httpContext.Request.Headers["Accept-Language"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(acceptLanguage))
                {
                    var language = acceptLanguage.Split(',')[0].Split(';')[0].Trim();
                    return NormalizeLanguage(language);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get current language from context");
        }

        return DefaultLanguage;
    }

    private string NormalizeLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return DefaultLanguage;

        language = language.ToLowerInvariant().Trim();
        
        // Support both "vi-VN" and "vi"
        if (language.StartsWith("vi"))
            return VietnameseLanguage;
        
        if (language.StartsWith("en"))
            return DefaultLanguage;

        // Default to English for unknown languages
        return DefaultLanguage;
    }

    private CultureInfo GetCultureInfo(string language)
    {
        try
        {
            return language switch
            {
                VietnameseLanguage => new CultureInfo("vi-VN"),
                DefaultLanguage => new CultureInfo("en-US"),
                _ => new CultureInfo("en-US")
            };
        }
        catch
        {
            return new CultureInfo("en-US");
        }
    }
}

