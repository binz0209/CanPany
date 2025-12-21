using CanPany.Application.Interfaces.Services;
using CanPany.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CanPany.Application.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register I18N Service
        services.AddScoped<II18nService, I18nService>();
        
        // Register other application services here
        // This can be used to centralize service registration
        
        return services;
    }
}

