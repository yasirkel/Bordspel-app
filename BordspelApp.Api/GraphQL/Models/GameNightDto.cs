using System;
using System.Collections.Generic;

namespace BordspelApp.Api.GraphQL.Models;

public record BoardGameDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Genre { get; init; }
}

public record GameNightDto
{
    public int Id { get; init; }
    public DateTime DateTime { get; init; }
    public string? Title { get; init; }
    public string? OrganizerName { get; init; }
    public IReadOnlyList<BoardGameDto> BoardGames { get; init; } = Array.Empty<BoardGameDto>();
}
