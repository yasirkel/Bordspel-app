using BordspelApp.Domain.Entities;

namespace BordspelApp.Application.Interfaces;

public interface IGameNightRepository
{
    Task<GameNight?> GetByIdAsync(int id);
    Task<GameNight?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<GameNight>> GetAllAsync();
    Task<IEnumerable<GameNight>> GetAllWithDetailsAsync();
    IQueryable<GameNight> GetAllQueryable();
    Task<GameNight> AddAsync(GameNight gameNight);
    Task UpdateAsync(GameNight gameNight);
    Task DeleteAsync(int id);
    Task DeleteAsync(GameNight gameNight);
    Task<IEnumerable<GameNight>> GetUpcomingGameNightsAsync();
    Task<IEnumerable<GameNight>> GetGameNightsByOrganizerAsync(int organizerId);
    Task<IEnumerable<GameNight>> GetGameNightsByDateAsync(DateTime date);
    Task<bool> PersonHasGameNightOnDateAsync(int personId, DateTime date);
    Task<IEnumerable<GameNight>> GetGameNightsByParticipantAsync(int personId);
}
