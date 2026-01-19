using BordspelApp.Domain.Enums;

namespace BordspelApp.Domain.Entities;

public class BoardGame
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GameGenre Genre { get; set; }
    public GameType Type { get; set; }
    public bool Is18Plus { get; set; }
    public string? PhotoUrl { get; set; }

    // Navigation properties
    public ICollection<GameNightBoardGame> GameNights { get; set; } = new List<GameNightBoardGame>();
}
