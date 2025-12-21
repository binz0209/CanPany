namespace CanPany.Infrastructure.Persistence.MongoDb.Initialization;

public interface IMongoInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

