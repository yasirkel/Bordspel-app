using BordspelApp.Application.Interfaces;
using BordspelApp.Domain.Entities;
using BordspelApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BordspelApp.Infrastructure.Repositories;

public class PotluckRepository : IPotluckRepository
{
    private readonly BordspelAppDbContext _context;

    public PotluckRepository(BordspelAppDbContext context)
    {
        _context = context;
    }

    public Task<bool> GameNightExistsAsync(int gameNightId)
    {
        return _context.GameNights.AnyAsync(gameNight => gameNight.Id == gameNightId);
    }

    public Task<bool> IsParticipantAsync(int gameNightId, int personId)
    {
        return _context.GameNightPlayers.AnyAsync(player =>
            player.GameNightId == gameNightId && player.PersonId == personId);
    }

    public async Task<PotluckItem> AddAsync(PotluckItem item)
    {
        _context.PotluckItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<IReadOnlyList<PotluckItem>> GetByGameNightAsync(int gameNightId)
    {
        return await _context.PotluckItems
            .AsNoTracking()
            .Where(item => item.GameNightId == gameNightId)
            .OrderBy(item => item.Description)
            .ToListAsync();
    }
}
