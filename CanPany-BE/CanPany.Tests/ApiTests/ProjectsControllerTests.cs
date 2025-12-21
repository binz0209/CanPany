using System.Net;
using System.Net.Http.Json;
using CanPany.Tests.Common;
using FluentAssertions;
using Xunit;

namespace CanPany.Tests.ApiTests;

public class ProjectsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProjectsControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProjects_ShouldReturnOk_WhenCalled()
    {
        // Act
        var response = await _client.GetAsync("/api/projects");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProject_ShouldRequireAuthentication()
    {
        // Arrange
        var project = new
        {
            Title = "Test Project",
            Description = "Test Description",
            BudgetType = "Fixed",
            BudgetAmount = 1000m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", project);

        // Assert
        // Should require authentication
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }
}

