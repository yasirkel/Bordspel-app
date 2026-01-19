using BordspelApp.Application.Interfaces;
using BordspelApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<IEnumerable<BoardGame>>> GetAll()
    {
        var boardGames = await _boardGameRepository.GetAllAsync();
        return Ok(boardGames);
    }

    [HttpGet("{id}")]
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
    public async Task<ActionResult<IEnumerable<BoardGame>>> GetAdultGames()
    {
        var boardGames = await _boardGameRepository.GetAdultGamesAsync();
        return Ok(boardGames);
    }

    [HttpPost]
    public async Task<ActionResult<BoardGame>> Create(BoardGame boardGame)
    {
        var created = await _boardGameRepository.AddAsync(boardGame);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, BoardGame boardGame)
    {
        if (id != boardGame.Id)
        {
            return BadRequest();
        }

        await _boardGameRepository.UpdateAsync(boardGame);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _boardGameRepository.DeleteAsync(id);
        return NoContent();
    }
}
