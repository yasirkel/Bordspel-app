using System.ComponentModel.DataAnnotations;

namespace BordspelApp.WebApp.Models;

public class GameNightCreateViewModel
{
    [Required(ErrorMessage = "Titel is verplicht")]
    [StringLength(100, ErrorMessage = "Titel mag maximaal 100 tekens bevatten")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Beschrijving mag maximaal 500 tekens bevatten")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Startdatum en tijd zijn verplicht")]
    [FutureDate(ErrorMessage = "Startdatum moet in de toekomst liggen")]
    public DateTime StartAt { get; set; } = DateTime.Now.AddDays(7);
    
    public DateTime DateTime 
    { 
        get => StartAt; 
        set => StartAt = value; 
    }

    [Required(ErrorMessage = "Maximum aantal spelers is verplicht")]
    [Range(2, 20, ErrorMessage = "Maximum aantal spelers moet tussen 2 en 20 liggen")]
    public int MaxPlayers { get; set; } = 6;

    [Required(ErrorMessage = "Straat is verplicht")]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Huisnummer is verplicht")]
    public string HouseNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Stad is verplicht")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Postcode is verplicht")]
    public string PostalCode { get; set; } = string.Empty;

    // Catering options
    public bool HasLactoseFree { get; set; }
    public bool HasNutFree { get; set; }
    public bool HasVegetarian { get; set; }
    public bool HasAlcoholFree { get; set; }
    public bool IsPotluck { get; set; }
    
    // Alias properties for view compatibility
    public bool HasLactoseFreeOption 
    { 
        get => HasLactoseFree; 
        set => HasLactoseFree = value; 
    }
    public bool HasNutFreeOption 
    { 
        get => HasNutFree; 
        set => HasNutFree = value; 
    }
    public bool HasVegetarianOption 
    { 
        get => HasVegetarian; 
        set => HasVegetarian = value; 
    }
    public bool HasAlcoholFreeOption 
    { 
        get => HasAlcoholFree; 
        set => HasAlcoholFree = value; 
    }

    // Board game selection
    [Required(ErrorMessage = "Selecteer minimaal één bordspel")]
    [MinLength(1, ErrorMessage = "Selecteer minimaal één bordspel")]
    public List<int> SelectedBoardGameIds { get; set; } = new();

    // Available board games for dropdown
    public List<BoardGameSelectionItem> AvailableBoardGames { get; set; } = new();
    
    // Alias for view compatibility
    public List<BoardGameSelectionItem> AllBoardGames 
    { 
        get => AvailableBoardGames; 
        set => AvailableBoardGames = value; 
    }
}

public class GameNightEditViewModel : GameNightCreateViewModel
{
    public int Id { get; set; }
}

public class BoardGameSelectionItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public bool Is18Plus { get; set; }
}

// Custom validation attribute for future dates
public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime dateTime)
        {
            if (dateTime <= DateTime.Now)
            {
                return new ValidationResult(ErrorMessage ?? "De datum moet in de toekomst liggen");
            }
        }
        return ValidationResult.Success;
    }
}
