using System.Net;
using System.Net.Http.Json;
using CanPany.Api.Controllers;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace CanPany.Tests.ApiTests;

public class AuthControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturnToken_WhenValidRequest()
    {
        // Arrange
        var request = new
        {
            FullName = "Test User",
            Email = "newuser@example.com",
            Password = "Password123!",
            Role = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result.Should().NotBeNull();
        // Token should be present
        Assert.NotNull(result!.GetType().GetProperty("accessToken"));
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailExists()
    {
        // Arrange
        var request = new
        {
            FullName = "Test User",
            Email = "existing@example.com",
            Password = "Password123!",
            Role = "User"
        };

        // Register first time
        await _client.PostAsJsonAsync("/api/auth/register", request);

        // Try to register again with same email
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenValidCredentials()
    {
        // Arrange - Register first
        var registerRequest = new
        {
            FullName = "Test User",
            Email = "login@example.com",
            Password = "Password123!",
            Role = "User"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Act - Login
        var loginRequest = new
        {
            Email = "login@example.com",
            Password = "Password123!",
            RememberMe = false
        };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result.Should().NotBeNull();
        Assert.NotNull(result!.GetType().GetProperty("accessToken"));
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenInvalidCredentials()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnTokenWithLongExpiry_WhenRememberMeTrue()
    {
        // Arrange - Register first
        var registerRequest = new
        {
            FullName = "Test User",
            Email = "rememberme@example.com",
            Password = "Password123!",
            Role = "User"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Act - Login with RememberMe
        var loginRequest = new
        {
            Email = "rememberme@example.com",
            Password = "Password123!",
            RememberMe = true
        };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result.Should().NotBeNull();
        var expiresIn = (int)result!.GetType().GetProperty("expiresIn")!.GetValue(result)!;
        // RememberMe should give longer expiry (7 days = 604800 seconds)
        expiresIn.Should().BeGreaterThan(3600); // At least 1 hour
    }
}

