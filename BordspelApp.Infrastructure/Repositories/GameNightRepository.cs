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

    // Haalt een spelavond zonder navigatiegegevens op.
    public async Task<GameNight?> GetByIdAsync(int id)
    {
        return await _context.GameNights.FindAsync(id);
    }

    // Haalt een spelavond met organisator, deelnemers en spellen op.
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

    // Haalt alle spelavonden op zonder aanvullende relaties.
    public async Task<IEnumerable<GameNight>> GetAllAsync()
    {
        return await _context.GameNights.ToListAsync();
    }

    // Geeft een queryable terug voor samengestelde spelavondqueries.
    public IQueryable<GameNight> GetAllQueryable()
    {
        return _context.GameNights;
    }

    // Haalt alle spelavonden met de belangrijkste relaties op.
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

    // Slaat een nieuwe spelavond op.
    public async Task<GameNight> AddAsync(GameNight gameNight)
    {
        _context.GameNights.Add(gameNight);
        await _context.SaveChangesAsync();
        return gameNight;
    }

    // Slaat wijzigingen aan een spelavond op.
    public async Task UpdateAsync(GameNight gameNight)
    {
        _context.GameNights.Update(gameNight);
        await _context.SaveChangesAsync();
    }

    // Verwijdert een spelavond op basis van het ID.
    public async Task DeleteAsync(int id)
    {
        var gameNight = await GetByIdAsync(id);
        if (gameNight != null)
        {
            _context.GameNights.Remove(gameNight);
            await _context.SaveChangesAsync();
        }
    }

    // Verwijdert de meegegeven spelavond.
    public async Task DeleteAsync(GameNight gameNight)
    {
        _context.GameNights.Remove(gameNight);
        await _context.SaveChangesAsync();
    }

    // Haalt toekomstige avonden op in oplopende datumvolgorde.
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

    // Haalt de avonden van één organisator op.
    public async Task<IEnumerable<GameNight>> GetGameNightsByOrganizerAsync(int organizerId)
    {
        return await _context.GameNights
            .Include(gn => gn.Organizer)
            .Where(gn => gn.OrganizerId == organizerId)
            .OrderByDescending(gn => gn.DateTime)
            .ToListAsync();
    }

    // Haalt alle avonden op die op een specifieke kalenderdag vallen.
    public async Task<IEnumerable<GameNight>> GetGameNightsByDateAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _context.GameNights
            .Include(gn => gn.Organizer)
            .Where(gn => gn.DateTime >= startOfDay && gn.DateTime < endOfDay)
            .ToListAsync();
    }

    // Controleert of een persoon die dag al een deelname heeft.
    public async Task<bool> PersonHasGameNightOnDateAsync(int personId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _context.GameNightPlayers
            .AnyAsync(gnp => gnp.PersonId == personId &&
                           gnp.GameNight.DateTime >= startOfDay &&
                           gnp.GameNight.DateTime < endOfDay);
    }

    // Haalt alle spelavonden op waaraan een persoon deelneemt.
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
