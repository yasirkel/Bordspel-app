using BordspelApp.Application.DTOs;

namespace BordspelApp.Application.Interfaces;

public interface IPotluckService
{
    /// <summary>Registreert een item voor een deelnemer.</summary>
    Task<PotluckItemDto?> AddItemAsync(PotluckItemCreateDto dto);
    /// <summary>Haalt alle items van een spelavond op.</summary>
    Task<IReadOnlyList<PotluckItemDto>> GetItemsAsync(int gameNightId);
}
