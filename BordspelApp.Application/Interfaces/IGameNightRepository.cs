using BordspelApp.Domain.Entities;

namespace BordspelApp.Application.Interfaces;

public interface IGameNightRepository
{
    /// <summary>Haalt een spelavond op via het ID.</summary>
    Task<GameNight?> GetByIdAsync(int id);
    /// <summary>Haalt een spelavond met alle benodigde details op.</summary>
    Task<GameNight?> GetByIdWithDetailsAsync(int id);
    /// <summary>Haalt alle spelavonden op.</summary>
    Task<IEnumerable<GameNight>> GetAllAsync();
    Task<IEnumerable<GameNight>> GetAllWithDetailsAsync();
    /// <summary>Geeft een uitbreidbare query voor spelavonden terug.</summary>
    IQueryable<GameNight> GetAllQueryable();
    /// <summary>Slaat een nieuwe spelavond op.</summary>
    Task<GameNight> AddAsync(GameNight gameNight);
    /// <summary>Slaat wijzigingen aan een spelavond op.</summary>
    Task UpdateAsync(GameNight gameNight);
    /// <summary>Verwijdert een spelavond via het ID.</summary>
    Task DeleteAsync(int id);
    /// <summary>Verwijdert een bestaande spelavond.</summary>
    Task DeleteAsync(GameNight gameNight);
    Task<IEnumerable<GameNight>> GetUpcomingGameNightsAsync();
    Task<IEnumerable<GameNight>> GetGameNightsByOrganizerAsync(int organizerId);
    Task<IEnumerable<GameNight>> GetGameNightsByDateAsync(DateTime date);
    /// <summary>Controleert de regel van maximaal één deelname per dag.</summary>
    Task<bool> PersonHasGameNightOnDateAsync(int personId, DateTime date);
    /// <summary>Haalt de deelnames van een persoon op.</summary>
    Task<IEnumerable<GameNight>> GetGameNightsByParticipantAsync(int personId);
}
