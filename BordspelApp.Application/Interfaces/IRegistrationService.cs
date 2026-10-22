using BordspelApp.Application.Models;

namespace BordspelApp.Application.Interfaces;

public interface IRegistrationService
{
    /// <summary>Registreert een persoon na controle van alle deelnamevoorwaarden.</summary>
    Task<RegistrationResponse> RegisterAsync(int personId, int gameNightId, bool forceRegister = false);
    /// <summary>Verwijdert een bestaande deelname.</summary>
    Task<bool> UnregisterAsync(int personId, int gameNightId);
    /// <summary>Controleert of een persoon al deelneemt.</summary>
    Task<bool> IsRegisteredAsync(int personId, int gameNightId);
}
