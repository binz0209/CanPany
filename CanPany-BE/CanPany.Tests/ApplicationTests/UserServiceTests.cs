using CanPany.Application.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Services;
using CanPany.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ApplicationTests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IUserProfileService> _mockProfileService;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockProfileService = new Mock<IUserProfileService>();
        var mockLogger = new Mock<ILogger<UserService>>();
        var mockI18n = new Mock<II18nService>();
        // Setup default I18N behavior - return key if not found
        mockI18n.Setup(x => x.GetString(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<string, object[]>((key, args) => key);
        mockI18n.Setup(x => x.GetError(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<string, object[]>((key, args) => key);
        mockI18n.Setup(x => x.GetValidation(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<string, object[]>((key, args) => key);
        mockI18n.Setup(x => x.GetLogging(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<string, object[]>((key, args) => key);
        
        _userService = new UserService(_mockUserRepo.Object, _mockProfileService.Object, mockLogger.Object, mockI18n.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = "user123";
        var expectedUser = new User
        {
            Id = userId,
            Email = "test@example.com",
            FullName = "Test User"
        };

        _mockUserRepo.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Email.Should().Be("test@example.com");
        _mockUserRepo.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserNotExists()
    {
        // Arrange
        var userId = "nonexistent";
        _mockUserRepo.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_WhenEmailNotExists()
    {
        // Arrange
        var email = "newuser@example.com";
        var password = "Password123!";
        var fullName = "New User";
        var role = "User";

        _mockUserRepo.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        _mockUserRepo.Setup(r => r.InsertAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // UserProfileService.CreateProfileAsync is called internally, we don't need to mock it for this test

        // Act
        var result = await _userService.RegisterAsync(fullName, email, password, role);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        result.FullName.Should().Be(fullName);
        result.Role.Should().Be(role);
        result.PasswordHash.Should().NotBeNullOrEmpty();
        _mockUserRepo.Verify(r => r.GetByEmailAsync(email), Times.Once);
        _mockUserRepo.Verify(r => r.InsertAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenEmailExists()
    {
        // Arrange
        var email = "existing@example.com";
        var existingUser = new User { Email = email };

        _mockUserRepo.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
            await _userService.RegisterAsync("Test", email, "Password123!", "User"));

        _mockUserRepo.Verify(r => r.InsertAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ValidateUserAsync_ShouldReturnUser_WhenCredentialsValid()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Id = "user123",
            Email = email,
            PasswordHash = hashedPassword
        };

        _mockUserRepo.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.ValidateUserAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public async Task ValidateUserAsync_ShouldReturnNull_WhenUserNotExists()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _mockUserRepo.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.ValidateUserAsync(email, "Password123!");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateUserAsync_ShouldReturnNull_WhenPasswordInvalid()
    {
        // Arrange
        var email = "test@example.com";
        var correctPassword = "Password123!";
        var wrongPassword = "WrongPassword";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);
        var user = new User
        {
            Id = "user123",
            Email = email,
            PasswordHash = hashedPassword
        };

        _mockUserRepo.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.ValidateUserAsync(email, wrongPassword);

        // Assert
        result.Should().BeNull();
    }
}

