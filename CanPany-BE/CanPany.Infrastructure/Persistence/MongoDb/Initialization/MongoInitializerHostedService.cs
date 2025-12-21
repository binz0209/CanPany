using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Persistence.MongoDb.Initialization;

public class MongoInitializerHostedService : IHostedService
{
    private readonly IMongoInitializer _initializer;
    private readonly ILogger<MongoInitializerHostedService> _logger;

    public MongoInitializerHostedService(IMongoInitializer initializer, ILogger<MongoInitializerHostedService> logger)
    {
        _initializer = initializer;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _initializer.InitializeAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MongoDB. Please check your connection string and credentials.");
            _logger.LogWarning("Application will continue but MongoDB features may not work.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

