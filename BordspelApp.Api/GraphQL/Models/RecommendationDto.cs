namespace BordspelApp.Api.GraphQL.Models;

public record RecommendationDto
{
    public GameNightDto? GameNight { get; init; }
    public int CommonPlayersCount { get; init; }
}
