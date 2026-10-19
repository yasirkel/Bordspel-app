using BordspelApp.Domain.Entities;

namespace BordspelApp.Application.Interfaces;

public interface IPotluckRepository
{
    Task<bool> GameNightExistsAsync(int gameNightId);
    Task<bool> IsParticipantAsync(int gameNightId, int personId);
    Task<PotluckItem> AddAsync(PotluckItem item);
    Task<IReadOnlyList<PotluckItem>> GetByGameNightAsync(int gameNightId);
}
