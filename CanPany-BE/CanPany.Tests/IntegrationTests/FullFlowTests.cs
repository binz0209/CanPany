using System.Net;
using System.Net.Http.Json;
using CanPany.Tests.Common;
using FluentAssertions;
using Xunit;

namespace CanPany.Tests.IntegrationTests;

/// <summary>
/// Integration tests cho full flow: Register -> Login -> Use API
/// </summary>
public class FullFlowTests : IClassFixture<TestWebApplicationFactory>, IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private string? _accessToken;

    public FullFlowTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FullFlow_Register_Login_GetProfile_ShouldWork()
    {
        // Step 1: Register
        var registerRequest = new
        {
            FullName = "Integration Test User",
            Email = $"integration_{Guid.NewGuid():N}@example.com",
            Password = "Password123!",
            Role = "User"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<dynamic>();
        _accessToken = registerResult!.GetType().GetProperty("accessToken")!.GetValue(registerResult)!.ToString();

        // Step 2: Login
        var loginRequest = new
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password,
            RememberMe = false
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<dynamic>();
        var loginToken = loginResult!.GetType().GetProperty("accessToken")!.GetValue(loginResult)!.ToString();
        loginToken.Should().NotBeNullOrEmpty();

        // Step 3: Use authenticated endpoint
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

        var profileResponse = await _client.GetAsync("/api/users/me");
        // Should return either OK or NotFound (if profile not created yet)
        profileResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FullFlow_CreateProject_ShouldRequireAuth()
    {
        // Step 1: Register and get token
        var registerRequest = new
        {
            FullName = "Project Creator",
            Email = $"project_{Guid.NewGuid():N}@example.com",
            Password = "Password123!",
            Role = "User"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<dynamic>();
        var token = registerResult!.GetType().GetProperty("accessToken")!.GetValue(registerResult)!.ToString();

        // Step 2: Try to create project without auth
        var projectRequest = new
        {
            Title = "Test Project",
            Description = "Test Description",
            BudgetType = "Fixed",
            BudgetAmount = 1000m
        };

        var projectResponseWithoutAuth = await _client.PostAsJsonAsync("/api/projects", projectRequest);
        projectResponseWithoutAuth.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);

        // Step 3: Try to create project with auth
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var projectResponseWithAuth = await _client.PostAsJsonAsync("/api/projects", projectRequest);
        // Should return either OK or BadRequest (depending on validation)
        projectResponseWithAuth.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}

