using BordspelApp.Application.DTOs;

namespace BordspelApp.Application.Interfaces;

public interface IBoardGameService
{
    Task<IEnumerable<BoardGameSimpleDto>> GetAllAsync();
    Task<BoardGameDto?> GetDetailsAsync(int id);
}
