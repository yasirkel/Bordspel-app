using BordspelApp.Application.DTOs;
using BordspelApp.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BordspelApp.Infrastructure.Services;

public class BoardGameService : IBoardGameService
{
    private readonly IBoardGameRepository _boardGameRepository;

    public BoardGameService(IBoardGameRepository boardGameRepository)
    {
        _boardGameRepository = boardGameRepository;
    }

    /// <summary>Haalt alle bordspellen op en zet ze om naar compacte weergavegegevens.</summary>
    public async Task<IEnumerable<BoardGameSimpleDto>> GetAllAsync()
    {
        var boardGames = await _boardGameRepository.GetAllQueryable()
            .AsNoTracking()
            .OrderBy(bg => bg.Name)
            .ToListAsync();

        return boardGames.Select(bg => new BoardGameSimpleDto
        {
            Id = bg.Id,
            Name = bg.Name,
            Genre = bg.Genre.ToString(),
            Type = bg.Type.ToString(),
            Is18Plus = bg.Is18Plus,
            ImageUrl = bg.PhotoUrl
        });
    }

    /// <summary>Haalt één bordspel met de bijbehorende toekomstige spelavonden op.</summary>
    public async Task<BoardGameDto?> GetDetailsAsync(int id)
    {
        var boardGame = await _boardGameRepository.GetAllQueryable()
            .AsNoTracking()
            .Include(bg => bg.GameNights)
                .ThenInclude(gnbg => gnbg.GameNight)
                    .ThenInclude(gn => gn.Address)
            .FirstOrDefaultAsync(bg => bg.Id == id);

        if (boardGame == null) return null;

        var upcomingGameNights = boardGame.GameNights
            .Where(gnbg => gnbg.GameNight.DateTime >= DateTime.Now)
            .Select(gnbg => new UpcomingGameNightDto
            {
                Id = gnbg.GameNight.Id,
                Title = gnbg.GameNight.Title,
                StartAt = gnbg.GameNight.DateTime,
                City = gnbg.GameNight.Address.City
            })
            .OrderBy(gn => gn.StartAt)
            .ToList();

        return new BoardGameDto
        {
            Id = boardGame.Id,
            Name = boardGame.Name,
            Description = boardGame.Description,
            Genre = boardGame.Genre.ToString(),
            Type = boardGame.Type.ToString(),
            Is18Plus = boardGame.Is18Plus,
            ImageUrl = boardGame.PhotoUrl,
            UpcomingGameNights = upcomingGameNights
        };
    }
}
