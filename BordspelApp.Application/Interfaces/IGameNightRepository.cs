using BordspelApp.Domain.Entities;

namespace BordspelApp.Application.Interfaces;

public interface IGameNightRepository
{
    // Haalt een spelavond op via het ID.
    Task<GameNight?> GetByIdAsync(int id);
    // Haalt een spelavond met alle benodigde details op.
    Task<GameNight?> GetByIdWithDetailsAsync(int id);
    // Haalt alle spelavonden op.
    Task<IEnumerable<GameNight>> GetAllAsync();
    Task<IEnumerable<GameNight>> GetAllWithDetailsAsync();
    // Geeft een uitbreidbare query voor spelavonden terug.
    IQueryable<GameNight> GetAllQueryable();
    // Slaat een nieuwe spelavond op.
    Task<GameNight> AddAsync(GameNight gameNight);
    // Slaat wijzigingen aan een spelavond op.
    Task UpdateAsync(GameNight gameNight);
    // Verwijdert een spelavond via het ID.
    Task DeleteAsync(int id);
    // Verwijdert een bestaande spelavond.
    Task DeleteAsync(GameNight gameNight);
    Task<IEnumerable<GameNight>> GetUpcomingGameNightsAsync();
    Task<IEnumerable<GameNight>> GetGameNightsByOrganizerAsync(int organizerId);
    Task<IEnumerable<GameNight>> GetGameNightsByDateAsync(DateTime date);
    // Controleert de regel van maximaal één deelname per dag.
    Task<bool> PersonHasGameNightOnDateAsync(int personId, DateTime date);
    // Haalt de deelnames van een persoon op.
    Task<IEnumerable<GameNight>> GetGameNightsByParticipantAsync(int personId);
}
