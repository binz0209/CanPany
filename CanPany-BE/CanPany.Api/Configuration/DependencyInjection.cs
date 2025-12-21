using CanPany.Application.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Services;
using CanPany.Infrastructure.ExternalServices.BackgroundJobs;
using CanPany.Infrastructure.ExternalServices.Cloudinary;
using CanPany.Infrastructure.ExternalServices.Email;
using CanPany.Infrastructure.Identity.Jwt;
using CanPany.Infrastructure.Localization;
using CanPany.Infrastructure.Messaging.SignalR;
using CanPany.Infrastructure.Persistence.MongoDb.Context;
using CanPany.Infrastructure.Persistence.MongoDb.Initialization;
using CanPany.Infrastructure.Persistence.MongoDb.Options;
using CanPany.Infrastructure.Persistence.MongoDb.Repositories;
using StackExchange.Redis;

namespace CanPany.Api.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ========== I18N / Localization ==========
        services.AddHttpContextAccessor();
        services.AddScoped<ILocalizationService, LocalizationService>();

        // ========== MongoDb ==========
        services.Configure<MongoOptions>(configuration.GetSection("MongoDb"));
        services.AddSingleton<MongoDbContext>();
        services.AddSingleton<IMongoInitializer, MongoInitializer>();
        services.AddHostedService<MongoInitializerHostedService>();

        // ========== Repositories ==========
        // Note: Update these to use new namespace after moving repositories
        // For now, keep using old repositories until migration is complete
        services.AddScoped<IUserRepository>(sp =>
            new CanPany.Infrastructure.Repositories.UserRepository(sp.GetRequiredService<MongoDbContext>().Users));
        services.AddScoped<IUserProfileRepository>(sp =>
            new CanPany.Infrastructure.Repositories.UserProfileRepository(sp.GetRequiredService<MongoDbContext>().UserProfiles));
        // TODO: Add other repositories after migration

        // ========== External Services ==========
        // Email
        services.AddSingleton<EmailService>(sp =>
        {
            var smtpHost = configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
            var fromEmail = configuration["Email:FromEmail"] ?? "";
            var fromName = configuration["Email:FromName"] ?? "CanPany";
            var password = configuration["Email:Password"] ?? "";
            return new EmailService(smtpHost, smtpPort, fromEmail, fromName, password);
        });
        services.AddScoped<IEmailService, EmailServiceAdapter>();

        // Cloudinary
        services.Configure<CloudinaryOptions>(configuration.GetSection("Cloudinary"));
        services.AddScoped<IImageUploadService, CloudinaryImageUploadService>();

        // JWT
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        // Redis Background Jobs
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            return ConnectionMultiplexer.Connect(connectionString);
        });
        services.AddScoped<IBackgroundJobService, RedisBackgroundJobService>();
        services.AddScoped<SendEmailJobHandler>();
        services.AddScoped<ProcessPaymentJobHandler>();
        services.AddScoped<GenerateReportJobHandler>();
        
        var workerCount = configuration.GetValue<int>("BackgroundJobs:WorkerCount", 2);
        for (int i = 0; i < workerCount; i++)
        {
            services.AddHostedService<BackgroundJobWorker>();
        }

        // SignalR
        services.AddScoped<IRealtimeService, SignalRRealtimeService>();

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // I18N Service
        services.AddScoped<CanPany.Application.Interfaces.Services.II18nService, CanPany.Application.Services.I18nService>();
        
        // Application services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        // ... Add other application services

        return services;
    }
}

