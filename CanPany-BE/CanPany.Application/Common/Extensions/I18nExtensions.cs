using CanPany.Application.Interfaces.Services;

namespace CanPany.Application.Common.Extensions;

/// <summary>
/// Extension methods cho I18N Service để dễ sử dụng
/// </summary>
public static class I18nExtensions
{
    /// <summary>
    /// Lấy error message với key pattern: Error.[Module].[Action].[Type]
    /// </summary>
    public static string Error(this II18nService i18n, string module, string action, string type, params object[] args)
    {
        return i18n.GetError($"{module}.{action}.{type}", args);
    }

    /// <summary>
    /// Lấy success message với key pattern: Success.[Module].[Action]
    /// </summary>
    public static string Success(this II18nService i18n, string module, string action, params object[] args)
    {
        return i18n.GetSuccess($"{module}.{action}", args);
    }

    /// <summary>
    /// Lấy validation message với key pattern: Validation.[Module].[Type]
    /// </summary>
    public static string Validation(this II18nService i18n, string module, string type, params object[] args)
    {
        return i18n.GetValidation($"{module}.{type}", args);
    }

    /// <summary>
    /// Lấy logging message với key pattern: Logging.[Module].[Action].[Type]
    /// </summary>
    public static string Log(this II18nService i18n, string module, string action, string type, params object[] args)
    {
        return i18n.GetLogging($"{module}.{action}.{type}", args);
    }

    /// <summary>
    /// Lấy status message với key pattern: Status.[Type]
    /// </summary>
    public static string Status(this II18nService i18n, string type, params object[] args)
    {
        return i18n.GetStatus(type, args);
    }
}






