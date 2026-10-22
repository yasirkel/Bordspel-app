namespace BordspelApp.Domain.Entities;

public class GameNightPlayer
{
    public int GameNightId { get; set; }
    public GameNight GameNight { get; set; } = null!;

    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    // Constructor zonder parameters die EF Core gebruikt bij het laden van een registratie.
    public GameNightPlayer()
    {
    }

    // Maakt een registratie aan voor een persoon bij een spelavond.
    public GameNightPlayer(int gameNightId, int personId)
    {
        GameNightId = gameNightId;
        PersonId = personId;
        RegisteredAt = DateTime.UtcNow;
    }
}
