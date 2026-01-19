namespace BordspelApp.Domain.Entities;

public class GameNightPlayer
{
    public int GameNightId { get; set; }
    public GameNight GameNight { get; set; } = null!;

    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    // Parameterless constructor for EF Core
    public GameNightPlayer()
    {
    }

    // Constructor for creating registrations
    public GameNightPlayer(int gameNightId, int personId)
    {
        GameNightId = gameNightId;
        PersonId = personId;
        RegisteredAt = DateTime.UtcNow;
    }
}
