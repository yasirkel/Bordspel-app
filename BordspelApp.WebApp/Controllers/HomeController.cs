using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BordspelApp.WebApp.Models;
using BordspelApp.Application.Interfaces;
using System.Net.Http.Json;

namespace BordspelApp.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IGameNightService _gameNightService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Microsoft.AspNetCore.Identity.UserManager<BordspelApp.Infrastructure.Identity.ApplicationUser> _userManager;
    private readonly BordspelApp.Application.Interfaces.IPersonService _personService;

    public HomeController(ILogger<HomeController> logger, IGameNightService gameNightService, IHttpClientFactory httpClientFactory, Microsoft.AspNetCore.Identity.UserManager<BordspelApp.Infrastructure.Identity.ApplicationUser> userManager, BordspelApp.Application.Interfaces.IPersonService personService)
    {
        _logger = logger;
        _gameNightService = gameNightService;
        _httpClientFactory = httpClientFactory;
        _userManager = userManager;
        _personService = personService;
    }

    // Toont de homepage met openbare en aanbevolen spelavonden.
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

        // If user is signed in, ensure they are linked to a Person and request recommendations from API GraphQL
        if (User?.Identity?.IsAuthenticated == true)
        {
            var appUser = await _userManager.GetUserAsync(User);

            // Koppel een ontbrekende domeinpersoon op basis van het Identity-e-mailadres.
            if (appUser != null && appUser.PersonId == null)
            {
                try
                {
                    var email = appUser.Email;
                    if (!string.IsNullOrEmpty(email))
                    {
                        var person = await _personService.GetByEmailAsync(email);
                        if (person != null)
                        {
                            appUser.PersonId = person.Id;
                            await _userManager.UpdateAsync(appUser);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to auto-link ApplicationUser to Person record for user {UserId}", appUser?.Id);
                }
            }

            if (appUser?.PersonId != null)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient("api");
                    var gql = @"query($personId:Int!){ recommendedGameNights(personId:$personId) { gameNight { id title dateTime organizerName } commonPlayersCount } }";
                    var req = new { query = gql, variables = new { personId = appUser.PersonId ?? 0 } };
                    var res = await client.PostAsJsonAsync("graphql", req);
                    if (!res.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("GraphQL-aanbevelingen mislukten met status {Status}.", res.StatusCode);
                    }
                    else
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(await res.Content.ReadAsStringAsync());
                        if (doc.RootElement.TryGetProperty("data", out var data) && data.TryGetProperty("recommendedGameNights", out var recs))
                        {
                            var any = false;
                            foreach (var el in recs.EnumerateArray())
                            {
                                any = true;
                                var gn = el.GetProperty("gameNight");
                                var id = gn.GetProperty("id").GetInt32();
                                var title = gn.GetProperty("title").GetString() ?? string.Empty;
                                var dateTime = gn.GetProperty("dateTime").GetDateTime();
                                var organizer = gn.TryGetProperty("organizerName", out var on) ? on.GetString() ?? string.Empty : string.Empty;
                                var common = el.GetProperty("commonPlayersCount").GetInt32();

                                viewModel.Recommendations.Add(new RecommendationViewModel
                                {
                                    GameNight = new GameNightItemViewModel
                                    {
                                        Id = id,
                                        Title = title,
                                        StartAt = dateTime,
                                        OrganizerName = organizer
                                    },
                                    CommonPlayersCount = common
                                });
                            }

                            // Zonder aanbevelingen tonen we bewust geen willekeurige fallbacklijst.
                            if (!any)
                            {
                                _logger.LogInformation("GraphQL returned zero recommendations for person {PersonId}", appUser.PersonId);
                                // De view toont in dat geval een duidelijke melding.
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load recommendations from API");
                }
            }
        }

        return View(viewModel);
    }

    // Toont de privacy-informatie van de applicatie.
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    // Toont een gebruiksvriendelijke foutpagina zonder technische details.
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
