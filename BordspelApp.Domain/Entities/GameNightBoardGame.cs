namespace BordspelApp.Domain.Entities;

public class GameNightBoardGame
{
    public int GameNightId { get; set; }
    public GameNight GameNight { get; set; } = null!;

    public int BoardGameId { get; set; }
    public BoardGame BoardGame { get; set; } = null!;
}
