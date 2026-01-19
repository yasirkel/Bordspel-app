namespace BordspelApp.WebApp.Models;

public class RegistrationConfirmViewModel
{
    public int GameNightId { get; set; }
    public string GameNightTitle { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public List<string> DietaryWarnings { get; set; } = new();
}
