using CanPany.Application.Interfaces.Repositories;
using CanPany.Application.Services;
using CanPany.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace CanPany.Tests.ApplicationTests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _authService = new AuthService(_mockUserRepo.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnUser_WhenCredentialsValid()
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
        var result = await _authService.AuthenticateAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        _mockUserRepo.Verify(r => r.GetByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnNull_WhenUserNotExists()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _mockUserRepo.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.AuthenticateAsync(email, "Password123!");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnNull_WhenPasswordInvalid()
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
        var result = await _authService.AuthenticateAsync(email, wrongPassword);

        // Assert
        result.Should().BeNull();
    }
}
