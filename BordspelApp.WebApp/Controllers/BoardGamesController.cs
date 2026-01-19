using BordspelApp.Application.Interfaces;
using BordspelApp.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BordspelApp.WebApp.Controllers;

[AllowAnonymous]
public class BoardGamesController : Controller
{
    private readonly IBoardGameService _boardGameService;

    public BoardGamesController(IBoardGameService boardGameService)
    {
        _boardGameService = boardGameService;
    }

    // GET: /BoardGames
    public async Task<IActionResult> Index()
    {
        var boardGames = await _boardGameService.GetAllAsync();

        var viewModel = new BoardGameListViewModel
        {
            BoardGames = boardGames.Select(bg => new BoardGameCardViewModel
            {
                Id = bg.Id,
                Name = bg.Name,
                Genre = bg.Genre,
                Type = bg.Type,
                Is18Plus = bg.Is18Plus,
                ImageUrl = bg.ImageUrl
            }).ToList()
        };

        return View(viewModel);
    }

    // GET: /BoardGames/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var boardGame = await _boardGameService.GetDetailsAsync(id);
        if (boardGame == null)
        {
            return NotFound();
        }

        var viewModel = new BoardGameDetailsViewModel
        {
            Id = boardGame.Id,
            Name = boardGame.Name,
            Description = boardGame.Description,
            Genre = boardGame.Genre,
            Type = boardGame.Type,
            Is18Plus = boardGame.Is18Plus,
            ImageUrl = boardGame.ImageUrl,
            UpcomingGameNights = boardGame.UpcomingGameNights.Select(gn => new UpcomingGameNightViewModel
            {
                Id = gn.Id,
                Title = gn.Title,
                StartAt = gn.StartAt,
                City = gn.City
            }).ToList()
        };

        return View(viewModel);
    }
}
