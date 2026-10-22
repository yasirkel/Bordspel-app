using BordspelApp.Domain.Entities;

namespace BordspelApp.Application.Interfaces;

public interface IBoardGameRepository
{
    /// <summary>Haalt een bordspel op via het ID.</summary>
    Task<BoardGame?> GetByIdAsync(int id);
    /// <summary>Haalt alle bordspellen op.</summary>
    Task<IEnumerable<BoardGame>> GetAllAsync();
    /// <summary>Geeft een uitbreidbare query voor bordspellen terug.</summary>
    IQueryable<BoardGame> GetAllQueryable();
    /// <summary>Slaat een nieuw bordspel op.</summary>
    Task<BoardGame> AddAsync(BoardGame boardGame);
    /// <summary>Slaat wijzigingen aan een bordspel op.</summary>
    Task UpdateAsync(BoardGame boardGame);
    Task DeleteAsync(int id);
    /// <summary>Haalt alleen 18+-bordspellen op.</summary>
    Task<IEnumerable<BoardGame>> GetAdultGamesAsync();
}
