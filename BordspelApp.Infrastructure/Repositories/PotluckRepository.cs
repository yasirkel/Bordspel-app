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

    // Controleert of de doelspelavond bestaat.
    public Task<bool> GameNightExistsAsync(int gameNightId)
    {
        return _context.GameNights.AnyAsync(gameNight => gameNight.Id == gameNightId);
    }

    // Controleert of een persoon deelnemer is van de spelavond.
    public Task<bool> IsParticipantAsync(int gameNightId, int personId)
    {
        return _context.GameNightPlayers.AnyAsync(player =>
            player.GameNightId == gameNightId && player.PersonId == personId);
    }

    // Slaat een nieuw potluckitem op in de domeindatabase.
    public async Task<PotluckItem> AddAsync(PotluckItem item)
    {
        _context.PotluckItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    // Haalt de potluckitems van een spelavond gesorteerd op.
    public async Task<IReadOnlyList<PotluckItem>> GetByGameNightAsync(int gameNightId)
    {
        return await _context.PotluckItems
            .AsNoTracking()
            .Where(item => item.GameNightId == gameNightId)
            .OrderBy(item => item.Description)
            .ToListAsync();
    }
}
