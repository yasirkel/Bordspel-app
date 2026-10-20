using System;
using System.Net;
using System.Net.Http.Json;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using BordspelApp.Infrastructure.Identity;
using BordspelApp.Tests.TestHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BordspelApp.Tests.Integration;

public sealed class ApiSecurityIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ApiSecurityIntegrationTests(TestWebApplicationFactory factory)
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
        var testPassword = $"T-{Guid.NewGuid():N}!aA1";

        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync("api-test@example.com");
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = "api-test@example.com",
                    Email = "api-test@example.com",
                    EmailConfirmed = true,
                    PersonId = 1
                };
                var createResult = await userManager.CreateAsync(user, testPassword);
                Assert.True(createResult.Succeeded, string.Join("; ", createResult.Errors.Select(error => error.Description)));
            }
            Assert.True(await userManager.CheckPasswordAsync(user, testPassword));
        }

        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "api-test@example.com",
            password = testPassword
        });
        var loginBody = await login.Content.ReadAsStringAsync();
        Assert.True(login.StatusCode == HttpStatusCode.OK, $"Login failed with {login.StatusCode}: {loginBody}");

        var response = await client.PostAsync("/api/GameNights/1/join/2", null);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
