using BordspelApp.Domain.Entities;

namespace BordspelApp.Application.Interfaces;

public interface IBoardGameRepository
{
    Task<BoardGame?> GetByIdAsync(int id);
    Task<IEnumerable<BoardGame>> GetAllAsync();
    IQueryable<BoardGame> GetAllQueryable();
    Task<BoardGame> AddAsync(BoardGame boardGame);
    Task UpdateAsync(BoardGame boardGame);
    Task DeleteAsync(int id);
    Task<IEnumerable<BoardGame>> GetAdultGamesAsync();
}
