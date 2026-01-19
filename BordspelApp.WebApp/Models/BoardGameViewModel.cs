namespace BordspelApp.WebApp.Models;

public class BoardGameListViewModel
{
    public List<BoardGameCardViewModel> BoardGames { get; set; } = new();
}

public class BoardGameDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Genre { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Is18Plus { get; set; }
    public string? ImageUrl { get; set; }
    public List<UpcomingGameNightViewModel> UpcomingGameNights { get; set; } = new();
}

public class UpcomingGameNightViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public string City { get; set; } = string.Empty;
}
