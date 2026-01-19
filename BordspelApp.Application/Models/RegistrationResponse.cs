namespace BordspelApp.Application.Models;

public enum RegistrationResult
{
    Success,
    AlreadyRegistered,
    GameNightFull,
    AgeRestriction,
    AlreadyRegisteredSameDay,
    DietaryMismatch,
    GameNightNotFound,
    PersonNotFound
}

public class RegistrationResponse
{
    public RegistrationResult Result { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> DietaryWarnings { get; set; } = new();
    public bool RequiresConfirmation => DietaryWarnings.Any();
}
