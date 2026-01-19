namespace BordspelApp.WebApp.Models;

public class GameNightListViewModel
{
    public List<GameNightItemViewModel> GameNights { get; set; } = new();
    public string? SearchCity { get; set; }
    public string? SearchOrganizer { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 10;
}

public class GameNightItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public string City { get; set; } = string.Empty;
    public string OrganizerName { get; set; } = string.Empty;
    public int MaxPlayers { get; set; }
    public int CurrentPlayers { get; set; }
    public bool Is18Plus { get; set; }
    public bool IsFull { get; set; }
}
