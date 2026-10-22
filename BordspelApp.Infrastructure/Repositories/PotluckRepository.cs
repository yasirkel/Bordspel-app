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

    /// <summary>Controleert of de doelspelavond bestaat.</summary>
    public Task<bool> GameNightExistsAsync(int gameNightId)
    {
        return _context.GameNights.AnyAsync(gameNight => gameNight.Id == gameNightId);
    }

    /// <summary>Controleert of een persoon deelnemer is van de spelavond.</summary>
    public Task<bool> IsParticipantAsync(int gameNightId, int personId)
    {
        return _context.GameNightPlayers.AnyAsync(player =>
            player.GameNightId == gameNightId && player.PersonId == personId);
    }

    /// <summary>Slaat een nieuw potluckitem op in de domeindatabase.</summary>
    public async Task<PotluckItem> AddAsync(PotluckItem item)
    {
        _context.PotluckItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    /// <summary>Haalt de potluckitems van een spelavond gesorteerd op.</summary>
    public async Task<IReadOnlyList<PotluckItem>> GetByGameNightAsync(int gameNightId)
    {
        return await _context.PotluckItems
            .AsNoTracking()
            .Where(item => item.GameNightId == gameNightId)
            .OrderBy(item => item.Description)
            .ToListAsync();
    }
}
