using BordspelApp.Application.Interfaces;
using BordspelApp.Domain.Entities;
using BordspelApp.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BordspelApp.Api.Services;

namespace BordspelApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameNightsController : ControllerBase
{
    private readonly IGameNightRepository _gameNightRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IBoardGameRepository _boardGameRepository;
    private readonly CurrentUserAccessor _currentUser;

    public GameNightsController(IGameNightRepository gameNightRepository, IPersonRepository personRepository,
        IBoardGameRepository boardGameRepository, CurrentUserAccessor currentUser)
    {
        _gameNightRepository = gameNightRepository;
        _personRepository = personRepository;
        _boardGameRepository = boardGameRepository;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameNightDto>>> GetAll()
    {
        var query = _gameNightRepository.GetAllQueryable().AsNoTracking();

        var projected = query.Select(gn => new GameNightDto
        {
            Id = gn.Id,
            Title = gn.Title,
            DateTime = gn.DateTime,
            MaxPlayers = gn.MaxPlayers,
            Is18Plus = gn.Is18Plus,
            Organizer = gn.Organizer == null ? null : new PersonSummaryDto
            {
                Id = gn.Organizer.Id,
                FirstName = gn.Organizer.FirstName,
                LastName = gn.Organizer.LastName,
                Email = gn.Organizer.Email
            },
            Players = gn.Players.Select(p => new PersonSummaryDto
            {
                Id = p.Person != null ? p.Person.Id : p.PersonId,
                FirstName = p.Person != null ? p.Person.FirstName : null,
                LastName = p.Person != null ? p.Person.LastName : null,
                Email = p.Person != null ? p.Person.Email : null
            }).ToList(),
            BoardGames = gn.BoardGames.Select(bg => new BoardGameSummaryDto
            {
                Id = bg.BoardGame.Id,
                Name = bg.BoardGame.Name
            }).ToList(),
            Street = gn.Address != null ? gn.Address.Street : null,
            HouseNumber = gn.Address != null ? gn.Address.HouseNumber : null,
            PostalCode = gn.Address != null ? gn.Address.PostalCode : null,
            City = gn.Address != null ? gn.Address.City : null
        });

        var list = await projected.ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GameNight>> GetById(int id)
    {
        var gameNight = await _gameNightRepository.GetByIdWithDetailsAsync(id);
        if (gameNight == null)
        {
            return NotFound();
        }
        return Ok(gameNight);
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<IEnumerable<GameNight>>> GetUpcoming()
    {
        var gameNights = await _gameNightRepository.GetUpcomingGameNightsAsync();
        return Ok(gameNights);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<GameNight>> Create(CreateGameNightRequest request)
    {
        var personId = await _currentUser.GetPersonIdAsync(User);
        if (personId is null)
        {
            return Forbid();
        }

        var organizer = await _personRepository.GetByIdAsync(personId.Value);
        if (organizer == null)
        {
            return BadRequest("Organizer not found.");
        }

        if (!organizer.CanOrganizeGameNight())
        {
            return BadRequest("Organizer must be at least 18 years old.");
        }

        var gameNight = new GameNight(request.Title, request.Description, request.DateTime,
            request.Address, request.MaxPlayers, organizer.Id)
        {
            IsPotluck = request.IsPotluck
        };

        if (!gameNight.IsDateValid())
        {
            return BadRequest("Game night date must be in the future.");
        }

        foreach (var boardGameId in request.BoardGameIds.Distinct())
        {
            var boardGame = await _boardGameRepository.GetByIdAsync(boardGameId);
            if (boardGame == null) return BadRequest($"Board game {boardGameId} not found.");
            gameNight.AddBoardGame(boardGame);
        }
        if (gameNight.BoardGames.Count > 0) gameNight.Recalculate18Plus();

        var created = await _gameNightRepository.AddAsync(gameNight);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateGameNightRequest request)
    {
        var gameNight = await _gameNightRepository.GetByIdWithDetailsAsync(id);
        var personId = await _currentUser.GetPersonIdAsync(User);
        if (gameNight == null) return NotFound();
        if (personId != gameNight.OrganizerId && !User.IsInRole("Admin")) return Forbid();
        if (gameNight.Players.Count > 0) return Conflict("A game night with registered players cannot be changed.");
        if (request.DateTime <= DateTime.Now) return BadRequest("Game night date must be in the future.");

        gameNight.Title = request.Title;
        gameNight.Description = request.Description;
        gameNight.DateTime = request.DateTime;
        gameNight.MaxPlayers = request.MaxPlayers;
        gameNight.Address = request.Address;
        gameNight.IsPotluck = request.IsPotluck;
        gameNight.BoardGames.Clear();
        foreach (var boardGameId in request.BoardGameIds.Distinct())
        {
            var boardGame = await _boardGameRepository.GetByIdAsync(boardGameId);
            if (boardGame == null) return BadRequest($"Board game {boardGameId} not found.");
            gameNight.AddBoardGame(boardGame);
        }
        if (gameNight.BoardGames.Count > 0) gameNight.Recalculate18Plus();
        await _gameNightRepository.UpdateAsync(gameNight);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var gameNight = await _gameNightRepository.GetByIdWithDetailsAsync(id);
        var personId = await _currentUser.GetPersonIdAsync(User);
        if (gameNight == null) return NotFound();
        if (personId != gameNight.OrganizerId && !User.IsInRole("Admin")) return Forbid();
        if (gameNight.Players.Count > 0) return Conflict("A game night with registered players cannot be deleted.");
        await _gameNightRepository.DeleteAsync(gameNight);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id}/players")]
    public Task<IActionResult> JoinAsCurrentUser(int id) => JoinGameNight(id, null);

    [HttpPost("{id}/join/{personId}")]
    [Authorize]
    public async Task<IActionResult> JoinGameNight(int id, int? personId)
    {
        var currentPersonId = await _currentUser.GetPersonIdAsync(User);
        if (currentPersonId is null) return Forbid();
        if (personId.HasValue && personId.Value != currentPersonId.Value) return Forbid();
        var authenticatedPersonId = currentPersonId.Value;
        var gameNight = await _gameNightRepository.GetByIdWithDetailsAsync(id);
        if (gameNight == null)
        {
            return NotFound("Game night not found.");
        }

        var person = await _personRepository.GetByIdAsync(authenticatedPersonId);
        if (person == null)
        {
            return NotFound("Person not found.");
        }

        if (!gameNight.CanPersonJoin(person))
        {
            return BadRequest("Person cannot join this game night.");
        }

        // Check if person already has a game night on this date
        var hasGameNightOnDate = await _gameNightRepository.PersonHasGameNightOnDateAsync(authenticatedPersonId, gameNight.DateTime.Date);
        if (hasGameNightOnDate)
        {
            return BadRequest("Person already has a game night on this date.");
        }

        var gameNightPlayer = new GameNightPlayer
        {
            GameNightId = id,
            PersonId = authenticatedPersonId
        };

        gameNight.Players.Add(gameNightPlayer);
        await _gameNightRepository.UpdateAsync(gameNight);

        return Ok();
    }
}
