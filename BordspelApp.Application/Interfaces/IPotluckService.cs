using BordspelApp.Application.DTOs;

namespace BordspelApp.Application.Interfaces;

public interface IPotluckService
{
    Task<PotluckItemDto?> AddItemAsync(PotluckItemCreateDto dto);
    Task<IReadOnlyList<PotluckItemDto>> GetItemsAsync(int gameNightId);
}
