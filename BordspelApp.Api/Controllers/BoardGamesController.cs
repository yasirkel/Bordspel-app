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
    // Geeft alle bordspellen terug; deze route is openbaar leesbaar.
    public async Task<ActionResult<IEnumerable<BoardGame>>> GetAll()
    {
        var boardGames = await _boardGameRepository.GetAllAsync();
        return Ok(boardGames);
    }

    [HttpGet("{id}")]
    // Geeft één bordspel terug of HTTP 404 als het niet bestaat.
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
    // Geeft alleen bordspellen terug die als 18+ zijn gemarkeerd.
    public async Task<ActionResult<IEnumerable<BoardGame>>> GetAdultGames()
    {
        var boardGames = await _boardGameRepository.GetAdultGamesAsync();
        return Ok(boardGames);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    // Maakt als beheerder een nieuw bordspel aan.
    public async Task<ActionResult<BoardGame>> Create(BoardGame boardGame)
    {
        var created = await _boardGameRepository.AddAsync(boardGame);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    // Wijzigt als beheerder een bestaand bordspel.
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
    // Verwijdert als beheerder een bordspel op basis van het ID.
    public async Task<IActionResult> Delete(int id)
    {
        await _boardGameRepository.DeleteAsync(id);
        return NoContent();
    }
}
