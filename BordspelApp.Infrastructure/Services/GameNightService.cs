using BordspelApp.Application.DTOs;
using BordspelApp.Application.Interfaces;
using BordspelApp.Domain.Entities;
using BordspelApp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BordspelApp.Infrastructure.Services;

public class GameNightService : IGameNightService
{
    private readonly IGameNightRepository _gameNightRepository;
    private readonly IBoardGameRepository _boardGameRepository;

    public GameNightService(IGameNightRepository gameNightRepository, IBoardGameRepository boardGameRepository)
    {
        _gameNightRepository = gameNightRepository;
        _boardGameRepository = boardGameRepository;
    }

    // Haalt toekomstige spelavonden op met optionele filters en paginering.
    public async Task<IEnumerable<GameNightListItemDto>> GetAllUpcomingAsync(string? searchCity = null, string? searchOrganizer = null, int page = 1, int pageSize = 10)
    {
        var query = _gameNightRepository.GetAllQueryable()
            .AsNoTracking()
            .Include(gn => gn.Organizer)
            .Include(gn => gn.Address)
            .Include(gn => gn.Players)
            .Where(gn => gn.DateTime >= DateTime.Now);

        if (!string.IsNullOrWhiteSpace(searchCity))
        {
            query = query.Where(gn => gn.Address.City.Contains(searchCity));
        }

        if (!string.IsNullOrWhiteSpace(searchOrganizer))
        {
            query = query.Where(gn => 
                (gn.Organizer.FirstName + " " + gn.Organizer.LastName).Contains(searchOrganizer));
        }

        var gameNights = await query
            .OrderBy(gn => gn.DateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return gameNights.Select(gn => new GameNightListItemDto
        {
            Id = gn.Id,
            Title = gn.Title,
            StartAt = gn.DateTime,
            City = gn.Address.City,
            OrganizerName = $"{gn.Organizer.FirstName} {gn.Organizer.LastName}",
            MaxPlayers = gn.MaxPlayers,
            CurrentPlayers = gn.Players.Count,
            Is18Plus = gn.Is18Plus
        });
    }

    // Geeft het aantal toekomstige avonden terug dat bij de filters past.
    public async Task<int> GetTotalUpcomingCountAsync(string? searchCity = null, string? searchOrganizer = null)
    {
        var query = _gameNightRepository.GetAllQueryable()
            .AsNoTracking()
            .Include(gn => gn.Organizer)
            .Include(gn => gn.Address)
            .Where(gn => gn.DateTime >= DateTime.Now);

        if (!string.IsNullOrWhiteSpace(searchCity))
        {
            query = query.Where(gn => gn.Address.City.Contains(searchCity));
        }

        if (!string.IsNullOrWhiteSpace(searchOrganizer))
        {
            query = query.Where(gn => 
                (gn.Organizer.FirstName + " " + gn.Organizer.LastName).Contains(searchOrganizer));
        }

        return await query.CountAsync();
    }

    // Haalt de avonden op die door een organisator zijn aangemaakt.
    public async Task<IEnumerable<GameNightListItemDto>> GetMyGameNightsAsync(int organizerId)
    {
        var gameNights = await _gameNightRepository.GetAllQueryable()
            .AsNoTracking()
            .Include(gn => gn.Organizer)
            .Include(gn => gn.Address)
            .Include(gn => gn.Players)
            .Where(gn => gn.OrganizerId == organizerId)
            .OrderBy(gn => gn.DateTime)
            .ToListAsync();

        return gameNights.Select(gn => new GameNightListItemDto
        {
            Id = gn.Id,
            Title = gn.Title,
            StartAt = gn.DateTime,
            City = gn.Address.City,
            OrganizerName = $"{gn.Organizer.FirstName} {gn.Organizer.LastName}",
            MaxPlayers = gn.MaxPlayers,
            CurrentPlayers = gn.Players.Count,
            Is18Plus = gn.Is18Plus
        });
    }

    // Haalt de avonden op waaraan een persoon deelneemt.
    public async Task<IEnumerable<GameNightListItemDto>> GetParticipatingGameNightsAsync(int personId)
    {
        var gameNights = await _gameNightRepository.GetAllQueryable()
            .AsNoTracking()
            .Include(gn => gn.Organizer)
            .Include(gn => gn.Address)
            .Include(gn => gn.Players)
            .Where(gn => gn.Players.Any(p => p.PersonId == personId))
            .OrderBy(gn => gn.DateTime)
            .ToListAsync();

        return gameNights.Select(gn => new GameNightListItemDto
        {
            Id = gn.Id,
            Title = gn.Title,
            StartAt = gn.DateTime,
            City = gn.Address.City,
            OrganizerName = $"{gn.Organizer.FirstName} {gn.Organizer.LastName}",
            MaxPlayers = gn.MaxPlayers,
            CurrentPlayers = gn.Players.Count,
            Is18Plus = gn.Is18Plus
        });
    }

    // Laadt een spelavond met organisator, deelnemers, spellen en potluckitems.
    public async Task<GameNightDetailsDto?> GetDetailsAsync(int id)
    {
        var gameNight = await _gameNightRepository.GetAllQueryable()
            .AsNoTracking()
            .Include(gn => gn.Organizer)
            .Include(gn => gn.Address)
            .Include(gn => gn.Players)
                .ThenInclude(p => p.Person)
            .Include(gn => gn.BoardGames)
                .ThenInclude(gnbg => gnbg.BoardGame)
            .Include(gn => gn.PotluckItems)
            .FirstOrDefaultAsync(gn => gn.Id == id);

        if (gameNight == null) return null;

        return new GameNightDetailsDto
        {
            Id = gameNight.Id,
            Title = gameNight.Title,
            Description = gameNight.Description,
            StartAt = gameNight.DateTime,
            MaxPlayers = gameNight.MaxPlayers,
            Is18Plus = gameNight.Is18Plus,
            Street = gameNight.Address.Street,
            HouseNumber = gameNight.Address.HouseNumber,
            City = gameNight.Address.City,
            PostalCode = gameNight.Address.PostalCode,
            OrganizerId = gameNight.OrganizerId,
            OrganizerName = $"{gameNight.Organizer.FirstName} {gameNight.Organizer.LastName}",
            HasLactoseFree = gameNight.HasLactoseFreeOption,
            HasNutFree = gameNight.HasNutFreeOption,
            HasVegetarian = gameNight.HasVegetarianOption,
            HasAlcoholFree = gameNight.HasAlcoholFreeOption,
            IsPotluck = gameNight.IsPotluck,
            Players = gameNight.Players.Select(p => new PlayerDto
            {
                PersonId = p.PersonId,
                Name = $"{p.Person.FirstName} {p.Person.LastName}"
            }).ToList(),
            BoardGames = gameNight.BoardGames.Select(gnbg => new BoardGameSimpleDto
            {
                Id = gnbg.BoardGame.Id,
                Name = gnbg.BoardGame.Name,
                Genre = gnbg.BoardGame.Genre.ToString(),
                Type = gnbg.BoardGame.Type.ToString(),
                Is18Plus = gnbg.BoardGame.Is18Plus,
                ImageUrl = gnbg.BoardGame.PhotoUrl
            }).ToList(),
            PotluckItems = gameNight.PotluckItems.Select(item => new PotluckItemDto
            {
                Id = item.Id,
                Description = item.Description,
                IsLactoseFree = item.IsLactoseFree,
                IsNutFree = item.IsNutFree,
                IsVegetarian = item.IsVegetarian,
                IsAlcoholFree = item.IsAlcoholFree
            }).ToList()
        };
    }

    // Maakt een nieuwe spelavond en koppelt de gekozen bordspellen.
    public async Task<bool> CreateAsync(GameNightDetailsDto dto, List<int> boardGameIds)
    {
        var address = new Address(dto.Street, dto.HouseNumber, dto.City, dto.PostalCode);
        
        var gameNight = new GameNight(
            dto.Title,
            dto.Description,
            dto.StartAt,
            address,
            dto.MaxPlayers,
            dto.OrganizerId
        );

        gameNight.HasLactoseFreeOption = dto.HasLactoseFree;
        gameNight.HasNutFreeOption = dto.HasNutFree;
        gameNight.HasVegetarianOption = dto.HasVegetarian;
        gameNight.HasAlcoholFreeOption = dto.HasAlcoholFree;
        gameNight.IsPotluck = dto.IsPotluck;

        // Koppel de bordspellen die in het formulier zijn geselecteerd.
        var boardGames = await _boardGameRepository.GetAllQueryable()
            .Where(bg => boardGameIds.Contains(bg.Id))
            .ToListAsync();

        foreach (var boardGame in boardGames)
        {
            gameNight.AddBoardGame(boardGame);
        }

        gameNight.Recalculate18Plus();

        await _gameNightRepository.AddAsync(gameNight);
        return true;
    }

    // Wijzigt een spelavond zolang nog niemand is ingeschreven.
    public async Task<bool> UpdateAsync(GameNightDetailsDto dto, List<int> boardGameIds)
    {
        var gameNight = await _gameNightRepository.GetAllQueryable()
            .Include(gn => gn.Address)
            .Include(gn => gn.Players)
            .Include(gn => gn.BoardGames)
                .ThenInclude(gnbg => gnbg.BoardGame)
            .FirstOrDefaultAsync(gn => gn.Id == dto.Id);

        if (gameNight == null) return false;

        // Een avond met deelnemers mag volgens US02 niet meer worden aangepast.
        if (gameNight.Players.Any())
        {
            return false; // Cannot edit if players are registered
        }

        // Werk de algemene gegevens van de spelavond bij.
        gameNight.Title = dto.Title;
        gameNight.Description = dto.Description;
        gameNight.DateTime = dto.StartAt;
        gameNight.MaxPlayers = dto.MaxPlayers;
        
        // Werk het adres als één value object bij.
        gameNight.Address.UpdateAddress(dto.Street, dto.HouseNumber, dto.City, dto.PostalCode);
        
        // Werk de beschikbare dieetopties en potluckinstelling bij.
        gameNight.HasLactoseFreeOption = dto.HasLactoseFree;
        gameNight.HasNutFreeOption = dto.HasNutFree;
        gameNight.HasVegetarianOption = dto.HasVegetarian;
        gameNight.HasAlcoholFreeOption = dto.HasAlcoholFree;
        gameNight.IsPotluck = dto.IsPotluck;

        // Vervang de spelkoppelingen door de nieuwe selectie.
        gameNight.BoardGames.Clear();
        var boardGames = await _boardGameRepository.GetAllQueryable()
            .Where(bg => boardGameIds.Contains(bg.Id))
            .ToListAsync();

        foreach (var boardGame in boardGames)
        {
            gameNight.AddBoardGame(boardGame);
        }

        gameNight.Recalculate18Plus();

        await _gameNightRepository.UpdateAsync(gameNight);
        return true;
    }

    // Verwijdert een spelavond wanneer deze nog geen deelnemers heeft.
    public async Task<bool> DeleteAsync(int id)
    {
        var gameNight = await _gameNightRepository.GetAllQueryable()
            .Include(gn => gn.Players)
            .FirstOrDefaultAsync(gn => gn.Id == id);

        if (gameNight == null) return false;

        // Een avond met deelnemers mag volgens US02 niet worden verwijderd.
        if (gameNight.Players.Any())
        {
            return false; // Cannot delete if players are registered
        }

        await _gameNightRepository.DeleteAsync(gameNight);
        return true;
    }

    // Controleert of een spelavond nog gewijzigd of verwijderd mag worden.
    public async Task<bool> CanEditOrDeleteAsync(int id)
    {
        var gameNight = await _gameNightRepository.GetAllQueryable()
            .AsNoTracking()
            .Include(gn => gn.Players)
            .FirstOrDefaultAsync(gn => gn.Id == id);

        return gameNight != null && !gameNight.Players.Any();
    }
}
