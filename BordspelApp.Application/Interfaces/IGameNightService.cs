using BordspelApp.Application.DTOs;
using BordspelApp.Application.Models;

namespace BordspelApp.Application.Interfaces;

public interface IGameNightService
{
    Task<IEnumerable<GameNightListItemDto>> GetAllUpcomingAsync(string? searchCity = null, string? searchOrganizer = null, int page = 1, int pageSize = 10);
    Task<int> GetTotalUpcomingCountAsync(string? searchCity = null, string? searchOrganizer = null);
    Task<IEnumerable<GameNightListItemDto>> GetMyGameNightsAsync(int organizerId);
    Task<IEnumerable<GameNightListItemDto>> GetParticipatingGameNightsAsync(int personId);
    Task<GameNightDetailsDto?> GetDetailsAsync(int id);
    Task<bool> CreateAsync(GameNightDetailsDto dto, List<int> boardGameIds);
    Task<bool> UpdateAsync(GameNightDetailsDto dto, List<int> boardGameIds);
    Task<bool> DeleteAsync(int id);
    Task<bool> CanEditOrDeleteAsync(int id);
}
