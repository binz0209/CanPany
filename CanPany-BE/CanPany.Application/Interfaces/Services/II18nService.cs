namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// I18N Service - Hỗ trợ đa ngôn ngữ (Tiếng Anh và Tiếng Việt)
/// </summary>
public interface II18nService
{
    /// <summary>
    /// Lấy message theo key
    /// </summary>
    string GetString(string key, params object[] args);

    /// <summary>
    /// Lấy error message
    /// </summary>
    string GetError(string key, params object[] args);

    /// <summary>
    /// Lấy success message
    /// </summary>
    string GetSuccess(string key, params object[] args);

    /// <summary>
    /// Lấy validation message
    /// </summary>
    string GetValidation(string key, params object[] args);

    /// <summary>
    /// Lấy logging message
    /// </summary>
    string GetLogging(string key, params object[] args);

    /// <summary>
    /// Lấy status message
    /// </summary>
    string GetStatus(string key, params object[] args);

    /// <summary>
    /// Set language hiện tại
    /// </summary>
    void SetLanguage(string languageCode);

    /// <summary>
    /// Get language hiện tại
    /// </summary>
    string GetCurrentLanguage();
}




