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
    public bool IsPotluck { get; set; }

    // Navigation properties
    public ICollection<GameNightPlayer> Players { get; set; } = new List<GameNightPlayer>();
    public ICollection<GameNightBoardGame> BoardGames { get; set; } = new List<GameNightBoardGame>();
    public ICollection<PotluckItem> PotluckItems { get; set; } = new List<PotluckItem>();

    // Constructor zonder parameters die EF Core gebruikt bij het laden van een bestaande avond.
    public GameNight()
    {
    }

    // Maakt een nieuwe spelavond aan met de organisator en basisgegevens.
    public GameNight(string title, string? description, DateTime startAt, Address address, int maxPlayers, int organizerId)
    {
        Title = title;
        Description = description;
        DateTime = startAt;
        Address = address;
        MaxPlayers = maxPlayers;
        OrganizerId = organizerId;
    }

    // Voegt een bordspel toe en maakt de koppeling met deze spelavond.
    public void AddBoardGame(BoardGame boardGame)
    {
        BoardGames.Add(new GameNightBoardGame
        {
            GameNightId = Id,
            BoardGameId = boardGame.Id,
            BoardGame = boardGame
        });
    }

    // Bepaalt opnieuw of de avond 18+ is op basis van de gekozen spellen.
    public void Recalculate18Plus()
    {
        Is18Plus = BoardGames.Any(gn => gn.BoardGame.Is18Plus);
    }

    // Controleert of het maximale aantal deelnemers is bereikt.
    public bool IsFull()
    {
        return Players.Count >= MaxPlayers;
    }

    // Controleert alle domeinregels waaraan een nieuwe deelnemer moet voldoen.
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

        // Een deelnemer mag niet tweemaal aan dezelfde avond worden toegevoegd.
        if (Players.Any(p => p.PersonId == person.Id))
        {
            return false;
        }

        return true;
    }

    // Controleert of de gekoppelde organisator oud genoeg is om te organiseren.
    public bool IsOrganizerValid()
    {
        return Organizer.CanOrganizeGameNight();
    }

    // Controleert of de spelavond in de toekomst plaatsvindt.
    public bool IsDateValid()
    {
        return DateTime > DateTime.Now;
    }
}
