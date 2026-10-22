using BordspelApp.Application.DTOs;

namespace BordspelApp.Application.Interfaces;

public interface IBoardGameService
{
    /// <summary>Haalt de bordspellen voor een overzicht op.</summary>
    Task<IEnumerable<BoardGameSimpleDto>> GetAllAsync();
    /// <summary>Haalt één bordspel met toekomstige spelavonden op.</summary>
    Task<BoardGameDto?> GetDetailsAsync(int id);
}
