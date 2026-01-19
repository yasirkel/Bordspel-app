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

    public async Task<BoardGame?> GetByIdAsync(int id)
    {
        return await _context.BoardGames.FindAsync(id);
    }

    public async Task<IEnumerable<BoardGame>> GetAllAsync()
    {
        return await _context.BoardGames.ToListAsync();
    }

    public IQueryable<BoardGame> GetAllQueryable()
    {
        return _context.BoardGames;
    }

    public async Task<BoardGame> AddAsync(BoardGame boardGame)
    {
        _context.BoardGames.Add(boardGame);
        await _context.SaveChangesAsync();
        return boardGame;
    }

    public async Task UpdateAsync(BoardGame boardGame)
    {
        _context.BoardGames.Update(boardGame);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var boardGame = await GetByIdAsync(id);
        if (boardGame != null)
        {
            _context.BoardGames.Remove(boardGame);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<BoardGame>> GetAdultGamesAsync()
    {
        return await _context.BoardGames
            .Where(bg => bg.Is18Plus)
            .ToListAsync();
    }
}
