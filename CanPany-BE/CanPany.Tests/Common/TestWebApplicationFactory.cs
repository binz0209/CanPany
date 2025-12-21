using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CanPany.Infrastructure.Persistence.MongoDb.Context;
using CanPany.Application.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Infrastructure.Persistence.MongoDb.Repositories;
using CanPany.Infrastructure.Identity.Jwt;
using CanPany.Infrastructure.ExternalServices.Email;
using CanPany.Domain.Entities;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using CanPany.Api;

namespace CanPany.Tests.Common;

/// <summary>
/// WebApplicationFactory cho integration tests
/// </summary>
// Note: Program.cs uses top-level statements, so we use a marker class
public class TestWebApplicationFactory : WebApplicationFactory<CanPany.Api.ProgramMarker>
{
    private readonly TestDatabase _testDb;

    public TestWebApplicationFactory()
    {
        _testDb = new TestDatabase();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override configuration cho test
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "MongoDb:ConnectionString", "mongodb://localhost:27017" },
                { "MongoDb:DbName", _testDb.Database.DatabaseNamespace.DatabaseName },
                { "Jwt:Key", "TestJwtKeyForTestingPurposesOnly12345678901234567890" },
                { "Jwt:Issuer", "CanPanyTest" },
                { "Jwt:Audience", "CanPanyTestClient" }
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove production services
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(MongoDbContext));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add test database
            services.AddSingleton<IMongoDatabase>(_testDb.Database);
            
            // Create MongoOptions for test
            var mongoOptions = Microsoft.Extensions.Options.Options.Create(new CanPany.Infrastructure.Persistence.MongoDb.Options.MongoOptions
            {
                ConnectionString = "mongodb://localhost:27017",
                DbName = _testDb.Database.DatabaseNamespace.DatabaseName,
                Collections = new CanPany.Infrastructure.Persistence.MongoDb.Options.MongoOptions.CollectionsSection()
            });
            services.AddSingleton(mongoOptions);
            
            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CanPany.Infrastructure.Persistence.MongoDb.Options.MongoOptions>>();
                return new MongoDbContext(options);
            });

            // Register test repositories
            services.AddScoped<IUserRepository>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<UserRepository>>();
                return new UserRepository(sp.GetRequiredService<MongoDbContext>().Users, logger);
            });

            // Register test services
            services.AddSingleton<IJwtTokenService>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                return new JwtTokenService(config);
            });

            // Mock external services
            services.AddScoped<IEmailService>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<EmailServiceAdapter>>();
                var emailService = new EmailService("smtp.test.com", 587, "test@test.com", "Test", "password");
                return new EmailServiceAdapter(emailService);
            });

            services.AddScoped<IRealtimeService>(sp =>
            {
                // Mock SignalR service for tests
                return new MockRealtimeService();
            });
        });

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning);
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _testDb?.Dispose();
        }
        base.Dispose(disposing);
    }
}

/// <summary>
/// Mock RealtimeService cho tests
/// </summary>
public class MockRealtimeService : IRealtimeService
{
    public Task SendToUserAsync(string userId, Domain.Entities.Notification notification)
    {
        // Mock implementation - không làm gì trong test
        return Task.CompletedTask;
    }
}

