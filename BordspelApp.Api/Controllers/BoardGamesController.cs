using BordspelApp.Application.Interfaces;
using BordspelApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BordspelApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BoardGamesController : ControllerBase
{
    private readonly IBoardGameRepository _boardGameRepository;

    public BoardGamesController(IBoardGameRepository boardGameRepository)
    {
        _boardGameRepository = boardGameRepository;
    }

    [HttpGet]
    /// <summary>Geeft alle bordspellen terug; deze route is openbaar leesbaar.</summary>
    public async Task<ActionResult<IEnumerable<BoardGame>>> GetAll()
    {
        var boardGames = await _boardGameRepository.GetAllAsync();
        return Ok(boardGames);
    }

    [HttpGet("{id}")]
    /// <summary>Geeft één bordspel terug of HTTP 404 als het niet bestaat.</summary>
    public async Task<ActionResult<BoardGame>> GetById(int id)
    {
        var boardGame = await _boardGameRepository.GetByIdAsync(id);
        if (boardGame == null)
        {
            return NotFound();
        }
        return Ok(boardGame);
    }

    [HttpGet("adult")]
    /// <summary>Geeft alleen bordspellen terug die als 18+ zijn gemarkeerd.</summary>
    public async Task<ActionResult<IEnumerable<BoardGame>>> GetAdultGames()
    {
        var boardGames = await _boardGameRepository.GetAdultGamesAsync();
        return Ok(boardGames);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    /// <summary>Maakt als beheerder een nieuw bordspel aan.</summary>
    public async Task<ActionResult<BoardGame>> Create(BoardGame boardGame)
    {
        var created = await _boardGameRepository.AddAsync(boardGame);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    /// <summary>Wijzigt als beheerder een bestaand bordspel.</summary>
    public async Task<IActionResult> Update(int id, BoardGame boardGame)
    {
        if (id != boardGame.Id)
        {
            return BadRequest();
        }

        await _boardGameRepository.UpdateAsync(boardGame);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    /// <summary>Verwijdert als beheerder een bordspel op basis van het ID.</summary>
    public async Task<IActionResult> Delete(int id)
    {
        await _boardGameRepository.DeleteAsync(id);
        return NoContent();
    }
}
