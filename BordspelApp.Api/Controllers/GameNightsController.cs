using BordspelApp.Application.Interfaces;
using BordspelApp.Domain.Entities;
using BordspelApp.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BordspelApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameNightsController : ControllerBase
{
    private readonly IGameNightRepository _gameNightRepository;
    private readonly IPersonRepository _personRepository;

    public GameNightsController(IGameNightRepository gameNightRepository, IPersonRepository personRepository)
    {
        _gameNightRepository = gameNightRepository;
        _personRepository = personRepository;
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

    [HttpPost]
    public async Task<ActionResult<GameNight>> Create(GameNight gameNight)
    {
        var organizer = await _personRepository.GetByIdAsync(gameNight.OrganizerId);
        if (organizer == null)
        {
            return BadRequest("Organizer not found.");
        }

        if (!organizer.CanOrganizeGameNight())
        {
            return BadRequest("Organizer must be at least 18 years old.");
        }

        if (!gameNight.IsDateValid())
        {
            return BadRequest("Game night date must be in the future.");
        }

        var created = await _gameNightRepository.AddAsync(gameNight);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id}/join/{personId}")]
    public async Task<IActionResult> JoinGameNight(int id, int personId)
    {
        var gameNight = await _gameNightRepository.GetByIdWithDetailsAsync(id);
        if (gameNight == null)
        {
            return NotFound("Game night not found.");
        }

        var person = await _personRepository.GetByIdAsync(personId);
        if (person == null)
        {
            return NotFound("Person not found.");
        }

        if (!gameNight.CanPersonJoin(person))
        {
            return BadRequest("Person cannot join this game night.");
        }

        // Check if person already has a game night on this date
        var hasGameNightOnDate = await _gameNightRepository.PersonHasGameNightOnDateAsync(personId, gameNight.DateTime.Date);
        if (hasGameNightOnDate)
        {
            return BadRequest("Person already has a game night on this date.");
        }

        var gameNightPlayer = new GameNightPlayer
        {
            GameNightId = id,
            PersonId = personId
        };

        gameNight.Players.Add(gameNightPlayer);
        await _gameNightRepository.UpdateAsync(gameNight);

        return Ok();
    }
}
