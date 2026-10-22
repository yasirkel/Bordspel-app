using BordspelApp.Application.Interfaces;
using BordspelApp.Domain.Entities;
using BordspelApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BordspelApp.Infrastructure.Repositories;

public class GameNightRepository : IGameNightRepository
{
    private readonly BordspelAppDbContext _context;

    public GameNightRepository(BordspelAppDbContext context)
    {
        _context = context;
    }

    /// <summary>Haalt een spelavond zonder navigatiegegevens op.</summary>
    public async Task<GameNight?> GetByIdAsync(int id)
    {
        return await _context.GameNights.FindAsync(id);
    }

    /// <summary>Haalt een spelavond met organisator, deelnemers en spellen op.</summary>
    public async Task<GameNight?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.GameNights
            .Include(gn => gn.Organizer)
            .Include(gn => gn.Players)
                .ThenInclude(p => p.Person)
            .Include(gn => gn.BoardGames)
                .ThenInclude(bg => bg.BoardGame)
            .FirstOrDefaultAsync(gn => gn.Id == id);
    }

    /// <summary>Haalt alle spelavonden op zonder aanvullende relaties.</summary>
    public async Task<IEnumerable<GameNight>> GetAllAsync()
    {
        return await _context.GameNights.ToListAsync();
    }

    /// <summary>Geeft een queryable terug voor samengestelde spelavondqueries.</summary>
    public IQueryable<GameNight> GetAllQueryable()
    {
        return _context.GameNights;
    }

    /// <summary>Haalt alle spelavonden met de belangrijkste relaties op.</summary>
    public async Task<IEnumerable<GameNight>> GetAllWithDetailsAsync()
    {
        return await _context.GameNights
            .Include(gn => gn.Organizer)
            .Include(gn => gn.Players)
                .ThenInclude(p => p.Person)
            .Include(gn => gn.BoardGames)
                .ThenInclude(bg => bg.BoardGame)
            .ToListAsync();
    }

    /// <summary>Slaat een nieuwe spelavond op.</summary>
    public async Task<GameNight> AddAsync(GameNight gameNight)
    {
        _context.GameNights.Add(gameNight);
        await _context.SaveChangesAsync();
        return gameNight;
    }

    /// <summary>Slaat wijzigingen aan een spelavond op.</summary>
    public async Task UpdateAsync(GameNight gameNight)
    {
        _context.GameNights.Update(gameNight);
        await _context.SaveChangesAsync();
    }

    /// <summary>Verwijdert een spelavond op basis van het ID.</summary>
    public async Task DeleteAsync(int id)
    {
        var gameNight = await GetByIdAsync(id);
        if (gameNight != null)
        {
            _context.GameNights.Remove(gameNight);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>Verwijdert de meegegeven spelavond.</summary>
    public async Task DeleteAsync(GameNight gameNight)
    {
        _context.GameNights.Remove(gameNight);
        await _context.SaveChangesAsync();
    }

    /// <summary>Haalt toekomstige avonden op in oplopende datumvolgorde.</summary>
    public async Task<IEnumerable<GameNight>> GetUpcomingGameNightsAsync()
    {
        return await _context.GameNights
            .Include(gn => gn.Organizer)
            .Include(gn => gn.BoardGames)
                .ThenInclude(bg => bg.BoardGame)
            .Where(gn => gn.DateTime > DateTime.Now)
            .OrderBy(gn => gn.DateTime)
            .ToListAsync();
    }

    /// <summary>Haalt de avonden van één organisator op.</summary>
    public async Task<IEnumerable<GameNight>> GetGameNightsByOrganizerAsync(int organizerId)
    {
        return await _context.GameNights
            .Include(gn => gn.Organizer)
            .Where(gn => gn.OrganizerId == organizerId)
            .OrderByDescending(gn => gn.DateTime)
            .ToListAsync();
    }

    /// <summary>Haalt alle avonden op die op een specifieke kalenderdag vallen.</summary>
    public async Task<IEnumerable<GameNight>> GetGameNightsByDateAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _context.GameNights
            .Include(gn => gn.Organizer)
            .Where(gn => gn.DateTime >= startOfDay && gn.DateTime < endOfDay)
            .ToListAsync();
    }

    /// <summary>Controleert of een persoon die dag al een deelname heeft.</summary>
    public async Task<bool> PersonHasGameNightOnDateAsync(int personId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _context.GameNightPlayers
            .AnyAsync(gnp => gnp.PersonId == personId &&
                           gnp.GameNight.DateTime >= startOfDay &&
                           gnp.GameNight.DateTime < endOfDay);
    }

    /// <summary>Haalt alle spelavonden op waaraan een persoon deelneemt.</summary>
    public async Task<IEnumerable<GameNight>> GetGameNightsByParticipantAsync(int personId)
    {
        // Verzamel eerst de IDs en laad daarna de volledige entiteiten met hun relaties.
        var ids = await _context.GameNightPlayers
            .Where(p => p.PersonId == personId)
            .Select(p => p.GameNightId)
            .Distinct()
            .ToListAsync();

        if (!ids.Any()) return Array.Empty<GameNight>();

        return await _context.GameNights
            .Include(gn => gn.Organizer)
            .Include(gn => gn.Players)
                .ThenInclude(p => p.Person)
            .Include(gn => gn.BoardGames)
                .ThenInclude(bg => bg.BoardGame)
            .Where(gn => ids.Contains(gn.Id))
            .ToListAsync();
    }
}
