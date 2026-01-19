namespace BordspelApp.Application.DTOs;

public class GameNightDetailsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartAt { get; set; }
    public int MaxPlayers { get; set; }
    public bool Is18Plus { get; set; }
    
    // Address
    public string Street { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    
    // Organizer
    public int OrganizerId { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    
    // Catering
    public bool HasLactoseFree { get; set; }
    public bool HasNutFree { get; set; }
    public bool HasVegetarian { get; set; }
    public bool HasAlcoholFree { get; set; }
    
    // Players
    public List<PlayerDto> Players { get; set; } = new();
    public int CurrentPlayers => Players.Count;
    public bool IsFull => CurrentPlayers >= MaxPlayers;
    
    // Board Games
    public List<BoardGameSimpleDto> BoardGames { get; set; } = new();
}

public class PlayerDto
{
    public int PersonId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class BoardGameSimpleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Is18Plus { get; set; }
    public string? ImageUrl { get; set; }
}
