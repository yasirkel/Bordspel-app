using BordspelApp.Application.DTOs;
using BordspelApp.Application.Models;

namespace BordspelApp.Application.Interfaces;

public interface IGameNightService
{
    // Haalt toekomstige spelavonden op met filters en paginering.
    Task<IEnumerable<GameNightListItemDto>> GetAllUpcomingAsync(string? searchCity = null, string? searchOrganizer = null, int page = 1, int pageSize = 10);
    // Geeft het aantal toekomstige spelavonden terug.
    Task<int> GetTotalUpcomingCountAsync(string? searchCity = null, string? searchOrganizer = null);
    // Haalt de eigen georganiseerde avonden op.
    Task<IEnumerable<GameNightListItemDto>> GetMyGameNightsAsync(int organizerId);
    // Haalt de avonden op waaraan een persoon deelneemt.
    Task<IEnumerable<GameNightListItemDto>> GetParticipatingGameNightsAsync(int personId);
    // Haalt de details van een spelavond op.
    Task<GameNightDetailsDto?> GetDetailsAsync(int id);
    // Maakt een spelavond met de geselecteerde bordspellen aan.
    Task<bool> CreateAsync(GameNightDetailsDto dto, List<int> boardGameIds);
    // Wijzigt een spelavond die nog geen deelnemers heeft.
    Task<bool> UpdateAsync(GameNightDetailsDto dto, List<int> boardGameIds);
    // Verwijdert een lege spelavond.
    Task<bool> DeleteAsync(int id);
    // Controleert of wijzigen of verwijderen nog is toegestaan.
    Task<bool> CanEditOrDeleteAsync(int id);
}
