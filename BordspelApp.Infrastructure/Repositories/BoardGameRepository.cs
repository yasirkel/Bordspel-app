using BordspelApp.Application.Interfaces;
using BordspelApp.Domain.Entities;
using BordspelApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BordspelApp.Infrastructure.Repositories;

public class BoardGameRepository : IBoardGameRepository
{
    private readonly BordspelAppDbContext _context;

    public BoardGameRepository(BordspelAppDbContext context)
    {
        _context = context;
    }

    /// <summary>Haalt één bordspel op via het primaire sleutelveld.</summary>
    public async Task<BoardGame?> GetByIdAsync(int id)
    {
        return await _context.BoardGames.FindAsync(id);
    }

    /// <summary>Haalt alle bordspellen op zonder extra relaties te laden.</summary>
    public async Task<IEnumerable<BoardGame>> GetAllAsync()
    {
        return await _context.BoardGames.ToListAsync();
    }

    /// <summary>Geeft een queryable terug voor filters en projecties in hogere lagen.</summary>
    public IQueryable<BoardGame> GetAllQueryable()
    {
        return _context.BoardGames;
    }

    /// <summary>Slaat een nieuw bordspel op in de domeindatabase.</summary>
    public async Task<BoardGame> AddAsync(BoardGame boardGame)
    {
        _context.BoardGames.Add(boardGame);
        await _context.SaveChangesAsync();
        return boardGame;
    }

    /// <summary>Slaat wijzigingen aan een bestaand bordspel op.</summary>
    public async Task UpdateAsync(BoardGame boardGame)
    {
        _context.BoardGames.Update(boardGame);
        await _context.SaveChangesAsync();
    }

    /// <summary>Verwijdert een bordspel als het gevonden wordt.</summary>
    public async Task DeleteAsync(int id)
    {
        var boardGame = await GetByIdAsync(id);
        if (boardGame != null)
        {
            _context.BoardGames.Remove(boardGame);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>Haalt uitsluitend bordspellen met de 18+-markering op.</summary>
    public async Task<IEnumerable<BoardGame>> GetAdultGamesAsync()
    {
        return await _context.BoardGames
            .Where(bg => bg.Is18Plus)
            .ToListAsync();
    }
}
