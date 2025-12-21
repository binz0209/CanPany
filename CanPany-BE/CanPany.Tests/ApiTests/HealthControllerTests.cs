using System.Net;
using System.Net.Http.Json;
using CanPany.Tests.Common;
using FluentAssertions;
using Xunit;

namespace CanPany.Tests.ApiTests;

public class HealthControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public HealthControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ShouldReturnOk_WhenCalled()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task HealthConfig_ShouldReturnConfigInfo()
    {
        // Act
        var response = await _client.GetAsync("/health/config");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result.Should().NotBeNull();
        // Check if config properties exist
        Assert.NotNull(result!.GetType().GetProperty("ok"));
        Assert.NotNull(result.GetType().GetProperty("mongo"));
    }

    [Fact]
    public async Task HealthMongo_ShouldReturnMongoStatus()
    {
        // Act
        var response = await _client.GetAsync("/health/mongo");

        // Assert
        // Should return either OK or 500 (depending on MongoDB connection)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result.Should().NotBeNull();
    }
}
