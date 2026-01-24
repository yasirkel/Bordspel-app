using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BordspelApp.Tests.Integration;

public class GameNightsIntegrationTests : IClassFixture<WebApplicationFactory<BordspelApp.Api.Program>>
{
    private readonly WebApplicationFactory<BordspelApp.Api.Program> _factory;

    public GameNightsIntegrationTests(WebApplicationFactory<BordspelApp.Api.Program> factory)
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
