using CanPany.Domain.Entities;
using CanPany.Infrastructure.Persistence.MongoDb.Context;
using CanPany.Infrastructure.Persistence.MongoDb.Repositories;
using CanPany.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Driver;
using Xunit;

namespace CanPany.Tests.InfrastructureTests;

public class UserRepositoryTests : IDisposable
{
    private readonly TestDatabase _testDb;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _testDb = new TestDatabase();
        var mongoOptions = Microsoft.Extensions.Options.Options.Create(new CanPany.Infrastructure.Persistence.MongoDb.Options.MongoOptions
        {
            ConnectionString = "mongodb://localhost:27017",
            DbName = _testDb.Database.DatabaseNamespace.DatabaseName,
            Collections = new CanPany.Infrastructure.Persistence.MongoDb.Options.MongoOptions.CollectionsSection()
        });
        var mongoContext = new MongoDbContext(mongoOptions);
        var mockLogger = new Mock<ILogger<UserRepository>>();
        _repository = new UserRepository(mongoContext.Users, mockLogger.Object);
    }

    [Fact]
    public async Task InsertAsync_ShouldCreateUser()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            FullName = "Test User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = "User"
        };

        // Act
        var result = await _repository.InsertAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            FullName = "Test User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = "User"
        };
        var inserted = await _repository.InsertAsync(user);

        // Act
        var result = await _repository.GetByIdAsync(inserted.Id!);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(inserted.Id);
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User
        {
            Email = email,
            FullName = "Test User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = "User"
        };
        await _repository.InsertAsync(user);

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            FullName = "Test User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = "User"
        };
        var inserted = await _repository.InsertAsync(user);
        inserted.FullName = "Updated Name";

        // Act
        var result = await _repository.UpdateAsync(inserted);

        // Assert
        result.Should().BeTrue();
        var updated = await _repository.GetByIdAsync(inserted.Id!);
        updated!.FullName.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteUser()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            FullName = "Test User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = "User"
        };
        var inserted = await _repository.InsertAsync(user);

        // Act
        var result = await _repository.DeleteAsync(inserted.Id!);

        // Assert
        result.Should().BeTrue();
        var deleted = await _repository.GetByIdAsync(inserted.Id!);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var user1 = new User { Email = "user1@example.com", FullName = "User 1", PasswordHash = "hash1", Role = "User" };
        var user2 = new User { Email = "user2@example.com", FullName = "User 2", PasswordHash = "hash2", Role = "User" };
        await _repository.InsertAsync(user1);
        await _repository.InsertAsync(user2);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Should().Contain(u => u.Email == "user1@example.com");
        result.Should().Contain(u => u.Email == "user2@example.com");
    }

    public void Dispose()
    {
        _testDb?.Dispose();
    }
}
