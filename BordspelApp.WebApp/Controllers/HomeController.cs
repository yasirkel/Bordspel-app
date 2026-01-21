using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BordspelApp.WebApp.Models;
using BordspelApp.Application.Interfaces;

namespace BordspelApp.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IGameNightService _gameNightService;

    public HomeController(ILogger<HomeController> logger, IGameNightService gameNightService)
    {
        _logger = logger;
        _gameNightService = gameNightService;
    }

    public async Task<IActionResult> Index()
    {
        // Load a few upcoming game nights from the database for the home page
        var gameNights = await _gameNightService.GetAllUpcomingAsync(page: 1, pageSize: 3);
        var viewModel = new GameNightListViewModel
        {
            GameNights = gameNights.Select(gn => new GameNightItemViewModel
            {
                Id = gn.Id,
                Title = gn.Title,
                StartAt = gn.StartAt,
                City = gn.City,
                OrganizerName = gn.OrganizerName,
                MaxPlayers = gn.MaxPlayers,
                CurrentPlayers = gn.CurrentPlayers,
                Is18Plus = gn.Is18Plus,
                IsFull = gn.IsFull
            }).ToList()
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
