using BordspelApp.Application.Models;

namespace BordspelApp.Application.Interfaces;

public interface IRegistrationService
{
    // Registreert een persoon na controle van alle deelnamevoorwaarden.
    Task<RegistrationResponse> RegisterAsync(int personId, int gameNightId, bool forceRegister = false);
    // Verwijdert een bestaande deelname.
    Task<bool> UnregisterAsync(int personId, int gameNightId);
    // Controleert of een persoon al deelneemt.
    Task<bool> IsRegisteredAsync(int personId, int gameNightId);
}
