using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BordspelApp.Tests.Integration;

public sealed class ApiSecurityIntegrationTests : IClassFixture<WebApplicationFactory<BordspelApp.Api.Program>>
{
    private readonly WebApplicationFactory<BordspelApp.Api.Program> _factory;

    public ApiSecurityIntegrationTests(WebApplicationFactory<BordspelApp.Api.Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task MutatingEndpoint_WithoutAuthentication_Returns401()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.PostAsJsonAsync("/api/GameNights/1/players", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_WithoutAuthentication_Returns401()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.PostAsJsonAsync("/api/Admin/link-user-person", new { email = "nobody@example.com", personId = 1 });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GraphQl_ExposesGameNightsAndBoardGames()
    {
        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/graphql", new { query = "{ gameNights { id title boardGames { id name genre } } boardGames { id name genre } }" });
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("errors", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("gameNights", body);
        Assert.Contains("boardGames", body);
    }

    [Fact]
    public async Task AuthenticatedUser_CannotJoinAsAnotherPerson()
    {
        var testPassword = Environment.GetEnvironmentVariable("BORDSPELAPP_TEST_PASSWORD");
        if (string.IsNullOrWhiteSpace(testPassword)) return;

        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "alice.dewit@example.com",
            password = testPassword
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var response = await client.PostAsync("/api/GameNights/1/join/2", null);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
