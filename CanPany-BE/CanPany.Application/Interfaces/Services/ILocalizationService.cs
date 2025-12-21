namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Service để quản lý đa ngôn ngữ (I18N) - Tiếng Anh và Tiếng Việt
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Lấy message theo key và language
    /// </summary>
    string GetString(string key, string? language = null);

    /// <summary>
    /// Lấy message với parameters
    /// </summary>
    string GetString(string key, object?[] parameters, string? language = null);

    /// <summary>
    /// Lấy error message
    /// </summary>
    string GetError(string errorKey, string? language = null);

    /// <summary>
    /// Lấy success message
    /// </summary>
    string GetSuccess(string successKey, string? language = null);

    /// <summary>
    /// Lấy validation message
    /// </summary>
    string GetValidation(string validationKey, string? language = null);

    /// <summary>
    /// Lấy status message
    /// </summary>
    string GetStatus(string statusKey, string? language = null);

    /// <summary>
    /// Lấy language từ HTTP context hoặc default
    /// </summary>
    string GetCurrentLanguage();
}

