namespace BordspelApp.WebApp.Models;

public class GameNightDetailsViewModel
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
    public bool IsPotluck { get; set; }
    
    // Players
    public List<PlayerViewModel> Players { get; set; } = new();
    public int CurrentPlayers => Players.Count;
    public bool IsFull => CurrentPlayers >= MaxPlayers;
    
    // Board Games
    public List<BoardGameCardViewModel> BoardGames { get; set; } = new();
    public List<PotluckItemViewModel> PotluckItems { get; set; } = new();
    
    // User state
    public bool IsUserRegistered { get; set; }
    public bool CanUserRegister { get; set; }
    public bool IsUserOrganizer { get; set; }
}

public class PlayerViewModel
{
    public int PersonId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class BoardGameCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Is18Plus { get; set; }
    public string? ImageUrl { get; set; }
}

public class PotluckItemViewModel
{
    public string Description { get; set; } = string.Empty;
    public bool IsLactoseFree { get; set; }
    public bool IsNutFree { get; set; }
    public bool IsVegetarian { get; set; }
    public bool IsAlcoholFree { get; set; }
}

public class PotluckItemInputModel
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vul minimaal één item in.")]
    [System.ComponentModel.DataAnnotations.StringLength(200)]
    public string Description { get; set; } = string.Empty;
    public bool IsLactoseFree { get; set; }
    public bool IsNutFree { get; set; }
    public bool IsVegetarian { get; set; }
    public bool IsAlcoholFree { get; set; }
}
