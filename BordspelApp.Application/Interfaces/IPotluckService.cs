using BordspelApp.Application.DTOs;

namespace BordspelApp.Application.Interfaces;

public interface IPotluckService
{
    // Registreert een item voor een deelnemer.
    Task<PotluckItemDto?> AddItemAsync(PotluckItemCreateDto dto);
    // Haalt alle items van een spelavond op.
    Task<IReadOnlyList<PotluckItemDto>> GetItemsAsync(int gameNightId);
}
