using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using BordspelApp.Tests.TestHelpers;
using Xunit;

namespace BordspelApp.Tests.Integration;

public class GameNightsIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public GameNightsIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetGameNights_Returns200()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/GameNights");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
