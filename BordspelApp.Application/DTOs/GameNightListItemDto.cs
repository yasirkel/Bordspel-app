namespace BordspelApp.Application.DTOs;

public class GameNightListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public string City { get; set; } = string.Empty;
    public string OrganizerName { get; set; } = string.Empty;
    public int MaxPlayers { get; set; }
    public int CurrentPlayers { get; set; }
    public bool Is18Plus { get; set; }
    public bool IsFull => CurrentPlayers >= MaxPlayers;
}
