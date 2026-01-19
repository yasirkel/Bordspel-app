using BordspelApp.Application.Models;

namespace BordspelApp.Application.Interfaces;

public interface IRegistrationService
{
    Task<RegistrationResponse> RegisterAsync(int personId, int gameNightId, bool forceRegister = false);
    Task<bool> UnregisterAsync(int personId, int gameNightId);
    Task<bool> IsRegisteredAsync(int personId, int gameNightId);
}
