using CanPany.Application.Common.Models;
using CanPany.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Extensions;

/// <summary>
/// Extension methods cho Controllers để dễ dàng sử dụng I18N và ApiResponse
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Trả về success response với I18N message
    /// </summary>
    public static IActionResult Ok<T>(this ControllerBase controller, T? data, string successKey, ILocalizationService? localization = null)
    {
        var language = localization?.GetCurrentLanguage() ?? "en";
        var message = localization?.GetSuccess(successKey, language) ?? successKey;
        return controller.Ok(ApiResponse<T>.CreateSuccess(data, message));
    }

    /// <summary>
    /// Trả về success response không có data
    /// </summary>
    public static IActionResult Ok(this ControllerBase controller, string successKey, ILocalizationService? localization = null)
    {
        var language = localization?.GetCurrentLanguage() ?? "en";
        var message = localization?.GetSuccess(successKey, language) ?? successKey;
        return controller.Ok(ApiResponse.CreateSuccess(message));
    }

    /// <summary>
    /// Trả về error response với I18N message
    /// </summary>
    public static IActionResult BadRequest(this ControllerBase controller, string errorKey, ILocalizationService? localization = null, string? errorCode = null, object?[]? parameters = null)
    {
        var language = localization?.GetCurrentLanguage() ?? "en";
        string message;
        if (parameters != null && parameters.Length > 0)
        {
            message = localization?.GetString(errorKey, parameters, language) ?? errorKey;
        }
        else
        {
            message = localization?.GetError(errorKey, language) ?? errorKey;
        }
        return controller.BadRequest(ApiResponse.CreateError(message, errorCode));
    }

    /// <summary>
    /// Trả về not found response với I18N message
    /// </summary>
    public static IActionResult NotFound(this ControllerBase controller, string errorKey, ILocalizationService? localization = null)
    {
        var language = localization?.GetCurrentLanguage() ?? "en";
        var message = localization?.GetError(errorKey, language) ?? errorKey;
        return controller.NotFound(ApiResponse.CreateError(message));
    }

    /// <summary>
    /// Trả về unauthorized response với I18N message
    /// </summary>
    public static IActionResult Unauthorized(this ControllerBase controller, string errorKey = "Unauthorized", ILocalizationService? localization = null)
    {
        var language = localization?.GetCurrentLanguage() ?? "en";
        var message = localization?.GetError(errorKey, language) ?? errorKey;
        return controller.Unauthorized(ApiResponse.CreateError(message));
    }
}

