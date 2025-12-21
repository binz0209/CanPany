using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Resources;

namespace CanPany.Application.Services;

/// <summary>
/// I18N Service Implementation - Hỗ trợ đa ngôn ngữ
/// </summary>
public class I18nService : II18nService
{
    private readonly ILogger<I18nService> _logger;
    private readonly ResourceManager _resourceManager;
    private string _currentLanguage = "en"; // Default: English

    public I18nService(ILogger<I18nService> logger)
    {
        _logger = logger;
        _resourceManager = new ResourceManager("CanPany.Application.Resources.Messages", typeof(I18nService).Assembly);
    }

    public string GetString(string key, params object[] args)
    {
        try
        {
            var culture = new CultureInfo(_currentLanguage);
            var message = _resourceManager.GetString(key, culture) ?? key;

            if (args != null && args.Length > 0)
            {
                message = string.Format(message, args);
            }

            return message;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get I18N string for key: {Key}", key);
            return key; // Return key if translation not found
        }
    }

    public string GetError(string key, params object[] args)
    {
        // Nếu key đã có prefix "Error." thì không thêm nữa
        if (key.StartsWith("Error.", StringComparison.OrdinalIgnoreCase))
            return GetString(key, args);
        return GetString($"Error.{key}", args);
    }

    public string GetSuccess(string key, params object[] args)
    {
        // Nếu key đã có prefix "Success." thì không thêm nữa
        if (key.StartsWith("Success.", StringComparison.OrdinalIgnoreCase))
            return GetString(key, args);
        return GetString($"Success.{key}", args);
    }

    public string GetValidation(string key, params object[] args)
    {
        // Nếu key đã có prefix "Validation." thì không thêm nữa
        if (key.StartsWith("Validation.", StringComparison.OrdinalIgnoreCase))
            return GetString(key, args);
        return GetString($"Validation.{key}", args);
    }

    public string GetLogging(string key, params object[] args)
    {
        // Nếu key đã có prefix "Logging." thì không thêm nữa
        if (key.StartsWith("Logging.", StringComparison.OrdinalIgnoreCase))
            return GetString(key, args);
        return GetString($"Logging.{key}", args);
    }

    public string GetStatus(string key, params object[] args)
    {
        // Nếu key đã có prefix "Status." thì không thêm nữa
        if (key.StartsWith("Status.", StringComparison.OrdinalIgnoreCase))
            return GetString(key, args);
        return GetString($"Status.{key}", args);
    }

    public void SetLanguage(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            languageCode = "en";

        _currentLanguage = languageCode.ToLower() switch
        {
            "vi" or "vietnamese" => "vi",
            "en" or "english" => "en",
            _ => "en"
        };
    }

    public string GetCurrentLanguage()
    {
        return _currentLanguage;
    }
}




