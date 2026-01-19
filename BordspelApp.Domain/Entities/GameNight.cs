using BordspelApp.Domain.ValueObjects;

namespace BordspelApp.Domain.Entities;

public class GameNight
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DateTime { get; set; }
    public int MaxPlayers { get; set; }
    public bool Is18Plus { get; private set; }

    // Organizer
    public int OrganizerId { get; set; }
    public Person Organizer { get; set; } = null!;

    // Address
    public Address Address { get; set; } = new();

    // Food and Drinks - Dietary Options Available
    public bool HasLactoseFreeOption { get; set; }
    public bool HasNutFreeOption { get; set; }
    public bool HasVegetarianOption { get; set; }
    public bool HasAlcoholFreeOption { get; set; }

    // Navigation properties
    public ICollection<GameNightPlayer> Players { get; set; } = new List<GameNightPlayer>();
    public ICollection<GameNightBoardGame> BoardGames { get; set; } = new List<GameNightBoardGame>();

    // Parameterless constructor for EF Core
    public GameNight()
    {
    }

    // Constructor for creating new game nights
    public GameNight(string title, string? description, DateTime startAt, Address address, int maxPlayers, int organizerId)
    {
        Title = title;
        Description = description;
        DateTime = startAt;
        Address = address;
        MaxPlayers = maxPlayers;
        OrganizerId = organizerId;
    }

    // Business Logic
    public void AddBoardGame(BoardGame boardGame)
    {
        BoardGames.Add(new GameNightBoardGame
        {
            GameNightId = Id,
            BoardGameId = boardGame.Id,
            BoardGame = boardGame
        });
    }

    public void Recalculate18Plus()
    {
        Is18Plus = BoardGames.Any(gn => gn.BoardGame.Is18Plus);
    }

    public bool IsFull()
    {
        return Players.Count >= MaxPlayers;
    }

    public bool CanPersonJoin(Person person)
    {
        if (IsFull())
        {
            return false;
        }

        if (Is18Plus && !person.IsAtLeast18YearsOld())
        {
            return false;
        }

        if (!person.IsAtLeast16YearsOld())
        {
            return false;
        }

        // Check if already registered
        if (Players.Any(p => p.PersonId == person.Id))
        {
            return false;
        }

        return true;
    }

    public bool IsOrganizerValid()
    {
        return Organizer.CanOrganizeGameNight();
    }

    public bool IsDateValid()
    {
        return DateTime > DateTime.Now;
    }
}
