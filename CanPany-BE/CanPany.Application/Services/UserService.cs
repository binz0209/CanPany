using CanPany.Application.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Constants;
using CanPany.Domain.Entities;
using CanPany.Domain.Exceptions;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IUserProfileService _profiles;
    private readonly IWalletService _wallet;
    private readonly ILogger<UserService> _logger;
    private readonly II18nService _i18n;

    public UserService(
        IUserRepository repo, 
        IUserProfileService profiles,
        IWalletService wallet,
        ILogger<UserService> logger,
        II18nService i18n)
    {
        _repo = repo;
        _profiles = profiles;
        _wallet = wallet;
        _logger = logger;
        _i18n = i18n;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("User ID cannot be null or empty", nameof(id));

            return await _repo.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
            throw;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            return await _repo.GetByEmailAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            throw;
        }
    }

    public async Task<User> RegisterAsync(string fullName, string email, string password, string role = "User")
    {
        try
        {
            _logger.LogInformation(_i18n.GetLogging(I18nKeys.Logging.User.Register.Start), email);

            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException(_i18n.GetValidation(I18nKeys.Validation.User.FullNameRequired), nameof(fullName));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException(_i18n.GetValidation(I18nKeys.Validation.User.EmailRequired), nameof(email));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException(_i18n.GetValidation(I18nKeys.Validation.User.PasswordRequired), nameof(password));

            var existing = await _repo.GetByEmailAsync(email);
            if (existing != null)
                throw new BusinessRuleViolationException("EmailExists", _i18n.GetError(I18nKeys.Error.User.Register.EmailExists, email));

            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _repo.InsertAsync(user);

            // ✅ Tạo UserProfile rỗng ngay sau khi đăng ký
            try
            {
                var emptyProfile = new UserProfile
                {
                    UserId = createdUser.Id,
                    Title = string.Empty,
                    Bio = string.Empty,
                    Location = string.Empty,
                    HourlyRate = null,
                    Languages = new List<string>(),
                    Certifications = new List<string>(),
                    SkillIds = new List<string>(),
                    CreatedAt = DateTime.UtcNow
                };

                await _profiles.CreateAsync(emptyProfile);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create user profile for user {UserId}, but user was created", createdUser.Id);
                // Don't throw - user is already created, profile can be created later
            }

            // ✅ Tự động tạo wallet cho user mới
            try
            {
                await _wallet.EnsureAsync(createdUser.Id);
                _logger.LogInformation("Created wallet for user {UserId}", createdUser.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create wallet for user {UserId}, but user was created", createdUser.Id);
                // Don't throw - user is already created, wallet can be created later
            }

            _logger.LogInformation(_i18n.GetLogging(I18nKeys.Logging.User.Register.Complete), email, createdUser.Id);
            return createdUser;
        }
        catch (DomainException)
        {
            throw; // Re-throw domain exceptions as-is
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user with email: {Email}", email);
            throw new InvalidDomainOperationException("RegisterUser", _i18n.GetError(I18nKeys.Error.User.Register.Failed, ex.Message), ex);
        }
    }

    public async Task<User?> ValidateUserAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation(_i18n.GetLogging(I18nKeys.Logging.User.Login.Start), email);

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException(_i18n.GetValidation(I18nKeys.Validation.User.EmailRequired), nameof(email));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException(_i18n.GetValidation(I18nKeys.Validation.User.PasswordRequired), nameof(password));

            var user = await _repo.GetByEmailAsync(email);
            if (user == null) return null;

            try
            {
                var isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                if (isValid)
                {
                    _logger.LogInformation(_i18n.GetLogging(I18nKeys.Logging.User.Login.Complete), email, user.Id);
                }
                return isValid ? user : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error verifying password for user: {Email}", email);
                return null; // Don't reveal that password verification failed due to error
            }
        }
        catch (ArgumentException)
        {
            throw; // Re-throw argument exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user with email: {Email}", email);
            throw new InvalidDomainOperationException("ValidateUser", _i18n.GetError(I18nKeys.Error.User.Login.Failed, ex.Message), ex);
        }
    }

    public async Task<bool> UpdateAsync(string id, User user)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("User ID cannot be null or empty", nameof(id));
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.Id = id;
            return await _repo.UpdateAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("User ID cannot be null or empty", nameof(id));

            return await _repo.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            return await _repo.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            throw new InvalidDomainOperationException("GetAllUsers", "Failed to retrieve users", ex);
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        var user = await _repo.GetByIdAsync(userId);
        if (user == null)
            return (false, new[] { "User not found" });

        var verify = BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash);
        if (!verify)
            return (false, new[] { "Old password is incorrect" });

        var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        var updated = await _repo.UpdatePasswordAsync(userId, newHash);

        return updated
            ? (true, Array.Empty<string>())
            : (false, new[] { "Failed to update password" });
    }

    public async Task UpdatePasswordAsync(string userId, string newPassword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("New password cannot be null or empty", nameof(newPassword));

            _logger.LogInformation(_i18n.GetLogging(I18nKeys.Logging.User.PasswordChange.Start), userId);

            var user = await _repo.GetByIdAsync(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            var updated = await _repo.UpdateAsync(user);
            
            if (!updated)
                throw new InvalidDomainOperationException("UpdatePassword", _i18n.GetError(I18nKeys.Error.User.PasswordChange.Failed, "Update failed"));
            
            _logger.LogInformation(_i18n.GetLogging(I18nKeys.Logging.User.PasswordChange.Complete), userId);
        }
        catch (DomainException)
        {
            throw; // Re-throw domain exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for user: {UserId}", userId);
            throw new InvalidDomainOperationException("UpdatePassword", _i18n.GetError(I18nKeys.Error.User.PasswordChange.Failed, ex.Message), ex);
        }
    }
}
