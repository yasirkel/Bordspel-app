using System;
using System.Collections.Generic;

namespace BordspelApp.Api.DTOs;

public class GameNightDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public DateTime DateTime { get; set; }
    public int MaxPlayers { get; set; }
    public bool Is18Plus { get; set; }

    public PersonSummaryDto? Organizer { get; set; }
    public List<PersonSummaryDto> Players { get; set; } = new();
    public List<BoardGameSummaryDto> BoardGames { get; set; } = new();

    // Address fields
    public string? Street { get; set; }
    public string? HouseNumber { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
}
