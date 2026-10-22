using BordspelApp.Application.DTOs;

namespace BordspelApp.Application.Interfaces;

public interface IBoardGameService
{
    // Haalt de bordspellen voor een overzicht op.
    Task<IEnumerable<BoardGameSimpleDto>> GetAllAsync();
    // Haalt één bordspel met toekomstige spelavonden op.
    Task<BoardGameDto?> GetDetailsAsync(int id);
}
