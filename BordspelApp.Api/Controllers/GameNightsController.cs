using BordspelApp.Application.Interfaces;
using BordspelApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<IEnumerable<GameNight>>> GetAll()
    {
        var gameNights = await _gameNightRepository.GetAllWithDetailsAsync();
        return Ok(gameNights);
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
