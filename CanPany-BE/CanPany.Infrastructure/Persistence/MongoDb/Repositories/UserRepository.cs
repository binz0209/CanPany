using CanPany.Application.Interfaces.Repositories;
using CanPany.Domain.Entities;
using CanPany.Domain.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Persistence.MongoDb.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _collection;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IMongoCollection<User> collection, ILogger<UserRepository> logger)
    {
        _collection = collection;
        _logger = logger;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            return await _collection.Find(_ => true).ToListAsync();
        }
        catch (MongoException ex)
        {
            _logger.LogError(ex, "MongoDB error getting all users");
            throw new InvalidDomainOperationException("GetAllUsers", "Failed to retrieve users from database", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting all users");
            throw;
        }
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        try
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }
        catch (MongoException ex)
        {
            _logger.LogError(ex, "MongoDB error getting user by ID: {UserId}", id);
            throw new InvalidDomainOperationException("GetUserById", $"Failed to retrieve user with ID '{id}'", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting user by ID: {UserId}", id);
            throw;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            return await _collection.Find(x => x.Email == email).FirstOrDefaultAsync();
        }
        catch (MongoException ex)
        {
            _logger.LogError(ex, "MongoDB error getting user by email: {Email}", email);
            throw new InvalidDomainOperationException("GetUserByEmail", $"Failed to retrieve user with email '{email}'", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting user by email: {Email}", email);
            throw;
        }
    }

    public async Task<User> InsertAsync(User entity)
    {
        try
        {
            await _collection.InsertOneAsync(entity);
            return entity;
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(ex, "Duplicate key error inserting user: {Email}", entity.Email);
            throw new BusinessRuleViolationException("EmailExists", $"User with email '{entity.Email}' already exists");
        }
        catch (MongoException ex)
        {
            _logger.LogError(ex, "MongoDB error inserting user: {Email}", entity.Email);
            throw new InvalidDomainOperationException("InsertUser", "Failed to create user in database", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error inserting user: {Email}", entity.Email);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(User entity)
    {
        try
        {
            var result = await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
            return result.ModifiedCount > 0;
        }
        catch (MongoException ex)
        {
            _logger.LogError(ex, "MongoDB error updating user: {UserId}", entity.Id);
            throw new InvalidDomainOperationException("UpdateUser", $"Failed to update user with ID '{entity.Id}'", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating user: {UserId}", entity.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var result = await _collection.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }
        catch (MongoException ex)
        {
            _logger.LogError(ex, "MongoDB error deleting user: {UserId}", id);
            throw new InvalidDomainOperationException("DeleteUser", $"Failed to delete user with ID '{id}'", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting user: {UserId}", id);
            throw;
        }
    }

    public async Task<bool> UpdatePasswordAsync(string id, string hashedPassword)
    {
        try
        {
            var update = Builders<User>.Update.Set(u => u.PasswordHash, hashedPassword);
            var result = await _collection.UpdateOneAsync(u => u.Id == id, update);
            return result.ModifiedCount > 0;
        }
        catch (MongoException ex)
        {
            _logger.LogError(ex, "MongoDB error updating password for user: {UserId}", id);
            throw new InvalidDomainOperationException("UpdatePassword", $"Failed to update password for user with ID '{id}'", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating password for user: {UserId}", id);
            throw;
        }
    }
}

