using BordspelApp.Domain.Entities;

namespace BordspelApp.Application.Interfaces;

public interface IBoardGameRepository
{
    // Haalt een bordspel op via het ID.
    Task<BoardGame?> GetByIdAsync(int id);
    // Haalt alle bordspellen op.
    Task<IEnumerable<BoardGame>> GetAllAsync();
    // Geeft een uitbreidbare query voor bordspellen terug.
    IQueryable<BoardGame> GetAllQueryable();
    // Slaat een nieuw bordspel op.
    Task<BoardGame> AddAsync(BoardGame boardGame);
    // Slaat wijzigingen aan een bordspel op.
    Task UpdateAsync(BoardGame boardGame);
    Task DeleteAsync(int id);
    // Haalt alleen 18+-bordspellen op.
    Task<IEnumerable<BoardGame>> GetAdultGamesAsync();
}
