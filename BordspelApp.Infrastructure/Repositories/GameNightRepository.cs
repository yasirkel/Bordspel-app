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

    public async Task<GameNight?> GetByIdAsync(int id)
    {
        return await _context.GameNights.FindAsync(id);
    }

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

    public async Task<IEnumerable<GameNight>> GetAllAsync()
    {
        return await _context.GameNights.ToListAsync();
    }

    public IQueryable<GameNight> GetAllQueryable()
    {
        return _context.GameNights;
    }

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

    public async Task<GameNight> AddAsync(GameNight gameNight)
    {
        _context.GameNights.Add(gameNight);
        await _context.SaveChangesAsync();
        return gameNight;
    }

    public async Task UpdateAsync(GameNight gameNight)
    {
        _context.GameNights.Update(gameNight);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var gameNight = await GetByIdAsync(id);
        if (gameNight != null)
        {
            _context.GameNights.Remove(gameNight);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(GameNight gameNight)
    {
        _context.GameNights.Remove(gameNight);
        await _context.SaveChangesAsync();
    }

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

    public async Task<IEnumerable<GameNight>> GetGameNightsByOrganizerAsync(int organizerId)
    {
        return await _context.GameNights
            .Include(gn => gn.Organizer)
            .Where(gn => gn.OrganizerId == organizerId)
            .OrderByDescending(gn => gn.DateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<GameNight>> GetGameNightsByDateAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _context.GameNights
            .Include(gn => gn.Organizer)
            .Where(gn => gn.DateTime >= startOfDay && gn.DateTime < endOfDay)
            .ToListAsync();
    }

    public async Task<bool> PersonHasGameNightOnDateAsync(int personId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _context.GameNightPlayers
            .AnyAsync(gnp => gnp.PersonId == personId &&
                           gnp.GameNight.DateTime >= startOfDay &&
                           gnp.GameNight.DateTime < endOfDay);
    }

    public async Task<IEnumerable<GameNight>> GetGameNightsByParticipantAsync(int personId)
    {
        // First collect the game night IDs for this participant, then query
        // GameNights with Includes. This avoids applying Include after a projection.
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
