using MongoDB.Driver;
using MongoDB.Bson;

namespace CanPany.Tests.Common;

/// <summary>
/// Helper class để tạo test database cho integration tests
/// </summary>
public class TestDatabase : IDisposable
{
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;
    private readonly string _databaseName;

    public TestDatabase(string connectionString = "mongodb://localhost:27017", string? databaseName = null)
    {
        _client = new MongoClient(connectionString);
        _databaseName = databaseName ?? $"CanPany_Test_{Guid.NewGuid():N}";
        _database = _client.GetDatabase(_databaseName);
    }

    public IMongoDatabase Database => _database;
    public IMongoClient Client => _client;

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }

    public void Dispose()
    {
        // Xóa test database sau khi test xong
        _client.DropDatabase(_databaseName);
    }
}

