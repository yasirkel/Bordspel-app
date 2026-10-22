using BordspelApp.Application.DTOs;
using BordspelApp.Application.Models;

namespace BordspelApp.Application.Interfaces;

public interface IGameNightService
{
    /// <summary>Haalt toekomstige spelavonden op met filters en paginering.</summary>
    Task<IEnumerable<GameNightListItemDto>> GetAllUpcomingAsync(string? searchCity = null, string? searchOrganizer = null, int page = 1, int pageSize = 10);
    /// <summary>Geeft het aantal toekomstige spelavonden terug.</summary>
    Task<int> GetTotalUpcomingCountAsync(string? searchCity = null, string? searchOrganizer = null);
    /// <summary>Haalt de eigen georganiseerde avonden op.</summary>
    Task<IEnumerable<GameNightListItemDto>> GetMyGameNightsAsync(int organizerId);
    /// <summary>Haalt de avonden op waaraan een persoon deelneemt.</summary>
    Task<IEnumerable<GameNightListItemDto>> GetParticipatingGameNightsAsync(int personId);
    /// <summary>Haalt de details van een spelavond op.</summary>
    Task<GameNightDetailsDto?> GetDetailsAsync(int id);
    /// <summary>Maakt een spelavond met de geselecteerde bordspellen aan.</summary>
    Task<bool> CreateAsync(GameNightDetailsDto dto, List<int> boardGameIds);
    /// <summary>Wijzigt een spelavond die nog geen deelnemers heeft.</summary>
    Task<bool> UpdateAsync(GameNightDetailsDto dto, List<int> boardGameIds);
    /// <summary>Verwijdert een lege spelavond.</summary>
    Task<bool> DeleteAsync(int id);
    /// <summary>Controleert of wijzigen of verwijderen nog is toegestaan.</summary>
    Task<bool> CanEditOrDeleteAsync(int id);
}
