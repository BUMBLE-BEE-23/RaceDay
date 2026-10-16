
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RaceDay.Tests;

public class ApiIntegrationTests
    : IClassFixture<RaceDayWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(RaceDayWebApplicationFactory factory)
    {
        _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
    }

    [Fact]
    public async Task GetEvents_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/api/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(
            "application/json",
            response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetMissingEvent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/events/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMyProfile_WithoutLogin_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/profile/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_WithoutLogin_ReturnsUnauthorized()
    {
        var request = new
        {
            Name = "Integration Test Race",
            Description = "Test event",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Johannesburg",
            DistanceKm = 5,
            EventType = "Run"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/events", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMyResults_WithoutLogin_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/results/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
