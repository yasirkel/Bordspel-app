using BordspelApp.Application.DTOs;
using BordspelApp.Application.Interfaces;
using BordspelApp.Application.Models;
using BordspelApp.WebApp.Models;
using BordspelApp.Domain.Entities;
using BordspelApp.Domain.Enums;
using BordspelApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BordspelApp.Domain.ValueObjects;

namespace BordspelApp.WebApp.Controllers;

public class GameNightsController : Controller
{
    private readonly IGameNightService _gameNightService;
    private readonly IRegistrationService _registrationService;
    private readonly IBoardGameService _boardGameService;
    private readonly IPersonService _personService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPersonRepository _personRepository;
    private readonly Microsoft.Extensions.Logging.ILogger<GameNightsController> _logger;

    public GameNightsController(
        IGameNightService gameNightService,
        IRegistrationService registrationService,
        IBoardGameService boardGameService,
        IPersonService personService,
        UserManager<ApplicationUser> userManager,
        IPersonRepository personRepository,
        Microsoft.Extensions.Logging.ILogger<GameNightsController> logger)
    {
        _gameNightService = gameNightService;
        _registrationService = registrationService;
        _boardGameService = boardGameService;
        _personService = personService;
        _userManager = userManager;
        _personRepository = personRepository;
        _logger = logger;
    }

    // GET: /GameNights/All
    [AllowAnonymous]
    public async Task<IActionResult> All(string? searchCity, string? searchOrganizer, int page = 1)
    {
        const int pageSize = 10;
        
        var gameNights = await _gameNightService.GetAllUpcomingAsync(searchCity, searchOrganizer, page, pageSize);
        var totalCount = await _gameNightService.GetTotalUpcomingCountAsync(searchCity, searchOrganizer);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

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
            }).ToList(),
            SearchCity = searchCity,
            SearchOrganizer = searchOrganizer,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = pageSize
        };

        return View(viewModel);
    }

    // GET: /GameNights/Mine
    [Authorize]
    public async Task<IActionResult> Mine()
    {
        var personId = await GetCurrentPersonIdAsync();
        if (personId == null)
        {
            TempData["Error"] = "Geen gekoppelde gebruiker gevonden.";
            return RedirectToAction(nameof(All));
        }

        var gameNights = await _gameNightService.GetMyGameNightsAsync(personId.Value);

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

    // GET: /GameNights/Participating
    [Authorize]
    public async Task<IActionResult> Participating()
    {
        var personId = await GetCurrentPersonIdAsync();
        if (personId == null)
        {
            TempData["Error"] = "Geen gekoppelde gebruiker gevonden.";
            return RedirectToAction(nameof(All));
        }

        var gameNights = await _gameNightService.GetParticipatingGameNightsAsync(personId.Value);

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

    // GET: /GameNights/Details/5
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var gameNight = await _gameNightService.GetDetailsAsync(id);
        if (gameNight == null)
        {
            return NotFound();
        }

        var personId = await GetCurrentPersonIdAsync();
        bool isRegistered = false;
        bool isOrganizer = false;

        if (personId.HasValue)
        {
            isRegistered = await _registrationService.IsRegisteredAsync(personId.Value, id);
            isOrganizer = gameNight.OrganizerId == personId.Value;
        }

        var viewModel = new GameNightDetailsViewModel
        {
            Id = gameNight.Id,
            Title = gameNight.Title,
            Description = gameNight.Description,
            StartAt = gameNight.StartAt,
            MaxPlayers = gameNight.MaxPlayers,
            Is18Plus = gameNight.Is18Plus,
            Street = gameNight.Street,
            HouseNumber = gameNight.HouseNumber,
            City = gameNight.City,
            PostalCode = gameNight.PostalCode,
            OrganizerId = gameNight.OrganizerId,
            OrganizerName = gameNight.OrganizerName,
            HasLactoseFree = gameNight.HasLactoseFree,
            HasNutFree = gameNight.HasNutFree,
            HasVegetarian = gameNight.HasVegetarian,
            HasAlcoholFree = gameNight.HasAlcoholFree,
            Players = gameNight.Players.Select(p => new PlayerViewModel
            {
                PersonId = p.PersonId,
                Name = p.Name
            }).ToList(),
            BoardGames = gameNight.BoardGames.Select(bg => new BoardGameCardViewModel
            {
                Id = bg.Id,
                Name = bg.Name,
                Genre = bg.Genre,
                Type = bg.Type,
                Is18Plus = bg.Is18Plus,
                ImageUrl = bg.ImageUrl
            }).ToList(),
            IsUserRegistered = isRegistered,
            CanUserRegister = personId.HasValue && !isRegistered && !gameNight.IsFull,
            IsUserOrganizer = isOrganizer
        };

        return View(viewModel);
    }

    // GET: /GameNights/Create
    [Authorize]
    public async Task<IActionResult> Create()
    {
        var personId = await GetCurrentPersonIdAsync();
        if (personId == null)
        {
            TempData["Error"] = "Geen gekoppelde gebruiker gevonden.";
            return RedirectToAction(nameof(All));
        }

        var age = await _personService.GetAgeAsync(personId.Value);
        if (age < 18)
        {
            TempData["Error"] = "Je moet minimaal 18 jaar zijn om een spelavond te organiseren.";
            return RedirectToAction(nameof(All));
        }

        var boardGames = await _boardGameService.GetAllAsync();
        var viewModel = new GameNightCreateViewModel
        {
            AvailableBoardGames = boardGames.Select(bg => new BoardGameSelectionItem
            {
                Id = bg.Id,
                Name = bg.Name,
                Is18Plus = bg.Is18Plus
            }).ToList()
        };

        return View(viewModel);
    }

    // POST: /GameNights/Create
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GameNightCreateViewModel model)
    {
        _logger.LogInformation("Create POST received: {@Model}", model);

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .Select(kvp => new { Key = kvp.Key, Errors = kvp.Value!.Errors.Select(e => e.ErrorMessage) });
            foreach(var e in errors) _logger.LogWarning("ModelState error: {Key} -> {Errors}", e.Key, string.Join(";", e.Errors));

            var boardGames = await _boardGameService.GetAllAsync();
            model.AvailableBoardGames = boardGames.Select(bg => new BoardGameSelectionItem
            {
                Id = bg.Id,
                Name = bg.Name,
                Is18Plus = bg.Is18Plus
            }).ToList();
            return View(model);
        }

        var personId = await GetCurrentPersonIdAsync();
        if (personId == null)
        {
            TempData["Error"] = "Geen gekoppelde gebruiker gevonden.";
            return RedirectToAction(nameof(All));
        }

        var age = await _personService.GetAgeAsync(personId.Value);
        if (age < 18)
        {
            TempData["Error"] = "Je moet minimaal 18 jaar zijn om een spelavond te organiseren.";
            return RedirectToAction(nameof(All));
        }

        var dto = new GameNightDetailsDto
        {
            Title = model.Title,
            Description = model.Description,
            StartAt = model.StartAt,
            MaxPlayers = model.MaxPlayers,
            Street = model.Street,
            HouseNumber = model.HouseNumber,
            City = model.City,
            PostalCode = model.PostalCode,
            OrganizerId = personId.Value,
            HasLactoseFree = model.HasLactoseFree,
            HasNutFree = model.HasNutFree,
            HasVegetarian = model.HasVegetarian,
            HasAlcoholFree = model.HasAlcoholFree
        };

        var success = await _gameNightService.CreateAsync(dto, model.SelectedBoardGameIds);
        _logger.LogInformation("GameNight create result: {Success}", success);
        if (success)
        {
            TempData["Success"] = "Spelavond succesvol aangemaakt!";
            return RedirectToAction(nameof(Mine));
        }

        TempData["Error"] = "Er ging iets mis bij het aanmaken van de spelavond.";
        return View(model);
    }

    // GET: /GameNights/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var personId = await GetCurrentPersonIdAsync();
        if (personId == null)
        {
            TempData["Error"] = "Geen gekoppelde gebruiker gevonden.";
            return RedirectToAction(nameof(All));
        }

        var gameNight = await _gameNightService.GetDetailsAsync(id);
        if (gameNight == null)
        {
            return NotFound();
        }

        if (gameNight.OrganizerId != personId.Value)
        {
            TempData["Error"] = "Je kunt alleen je eigen spelavonden bewerken.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var canEdit = await _gameNightService.CanEditOrDeleteAsync(id);
        if (!canEdit)
        {
            TempData["Error"] = "Deze spelavond kan niet meer worden bewerkt omdat er al spelers zijn ingeschreven.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var boardGames = await _boardGameService.GetAllAsync();
        var viewModel = new GameNightEditViewModel
        {
            Id = gameNight.Id,
            Title = gameNight.Title,
            Description = gameNight.Description,
            StartAt = gameNight.StartAt,
            MaxPlayers = gameNight.MaxPlayers,
            Street = gameNight.Street,
            HouseNumber = gameNight.HouseNumber,
            City = gameNight.City,
            PostalCode = gameNight.PostalCode,
            HasLactoseFree = gameNight.HasLactoseFree,
            HasNutFree = gameNight.HasNutFree,
            HasVegetarian = gameNight.HasVegetarian,
            HasAlcoholFree = gameNight.HasAlcoholFree,
            SelectedBoardGameIds = gameNight.BoardGames.Select(bg => bg.Id).ToList(),
            AvailableBoardGames = boardGames.Select(bg => new BoardGameSelectionItem
            {
                Id = bg.Id,
                Name = bg.Name,
                Is18Plus = bg.Is18Plus
            }).ToList()
        };
        
        // Store CanEdit in ViewBag for the view to access
        ViewBag.CanEdit = canEdit;

        return View(viewModel);
    }

    // POST: /GameNights/Edit/5
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GameNightEditViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            var boardGames = await _boardGameService.GetAllAsync();
            model.AvailableBoardGames = boardGames.Select(bg => new BoardGameSelectionItem
            {
                Id = bg.Id,
                Name = bg.Name,
                Is18Plus = bg.Is18Plus
            }).ToList();
            return View(model);
        }

        var personId = await GetCurrentPersonIdAsync();
        if (personId == null)
        {
            TempData["Error"] = "Geen gekoppelde gebruiker gevonden.";
            return RedirectToAction(nameof(All));
        }

        var existingGameNight = await _gameNightService.GetDetailsAsync(id);
        if (existingGameNight == null)
        {
            return NotFound();
        }

        if (existingGameNight.OrganizerId != personId.Value)
        {
            TempData["Error"] = "Je kunt alleen je eigen spelavonden bewerken.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var dto = new GameNightDetailsDto
        {
            Id = model.Id,
            Title = model.Title,
            Description = model.Description,
            StartAt = model.StartAt,
            MaxPlayers = model.MaxPlayers,
            Street = model.Street,
            HouseNumber = model.HouseNumber,
            City = model.City,
            PostalCode = model.PostalCode,
            OrganizerId = personId.Value,
            HasLactoseFree = model.HasLactoseFree,
            HasNutFree = model.HasNutFree,
            HasVegetarian = model.HasVegetarian,
            HasAlcoholFree = model.HasAlcoholFree
        };

        var success = await _gameNightService.UpdateAsync(dto, model.SelectedBoardGameIds);
        if (success)
        {
            TempData["Success"] = "Spelavond succesvol bijgewerkt!";
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Error"] = "Deze spelavond kan niet meer worden bewerkt omdat er al spelers zijn ingeschreven.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: /GameNights/Delete/5
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var personId = await GetCurrentPersonIdAsync();
        if (personId == null)
        {
            TempData["Error"] = "Geen gekoppelde gebruiker gevonden.";
            return RedirectToAction(nameof(All));
        }

        var gameNight = await _gameNightService.GetDetailsAsync(id);
        if (gameNight == null)
        {
            return NotFound();
        }

        if (gameNight.OrganizerId != personId.Value)
        {
            TempData["Error"] = "Je kunt alleen je eigen spelavonden verwijderen.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var canDelete = await _gameNightService.CanEditOrDeleteAsync(id);
        if (!canDelete)
        {
            TempData["Error"] = "Deze spelavond kan niet worden verwijderd omdat er al spelers zijn ingeschreven.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var viewModel = new GameNightDetailsViewModel
        {
            Id = gameNight.Id,
            Title = gameNight.Title,
            Description = gameNight.Description,
            StartAt = gameNight.StartAt,
            City = gameNight.City,
            OrganizerName = gameNight.OrganizerName
        };

        return View(viewModel);
    }

    // POST: /GameNights/Delete/5
    [Authorize]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var personId = await GetCurrentPersonIdAsync();
        if (personId == null)
        {
            TempData["Error"] = "Geen gekoppelde gebruiker gevonden.";
            return RedirectToAction(nameof(All));
        }

        var gameNight = await _gameNightService.GetDetailsAsync(id);
        if (gameNight == null)
        {
            return NotFound();
        }

        if (gameNight.OrganizerId != personId.Value)
        {
            TempData["Error"] = "Je kunt alleen je eigen spelavonden verwijderen.";
            return RedirectToAction(nameof(All));
        }

        var success = await _gameNightService.DeleteAsync(id);
        if (success)
        {
            TempData["Success"] = "Spelavond succesvol verwijderd!";
            return RedirectToAction(nameof(Mine));
        }

        TempData["Error"] = "Deze spelavond kan niet worden verwijderd omdat er al spelers zijn ingeschreven.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /GameNights/Register/5
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(int id)
    {
        var personId = await GetCurrentPersonIdAsync();
        if (personId == null)
        {
            TempData["Error"] = "Geen gekoppelde gebruiker gevonden.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var result = await _registrationService.RegisterAsync(personId.Value, id);

        if (result.Result == RegistrationResult.DietaryMismatch)
        {
            // Store warnings in TempData and redirect back to the details page
            // so warnings are displayed inline above the Aanmelden button.
            if (result.DietaryWarnings != null && result.DietaryWarnings.Any())
            {
                TempData["DietaryWarnings"] = string.Join("||", result.DietaryWarnings);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        if (result.Result == RegistrationResult.Success)
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /GameNights/ConfirmRegister
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmRegister(int gameNightId)
    {
        var personId = await GetCurrentPersonIdAsync();
        if (personId == null)
        {
            TempData["Error"] = "Geen gekoppelde gebruiker gevonden.";
            return RedirectToAction(nameof(Details), new { id = gameNightId });
        }

        var result = await _registrationService.RegisterAsync(personId.Value, gameNightId, forceRegister: true);

        if (result.Result == RegistrationResult.Success)
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }

        return RedirectToAction(nameof(Details), new { id = gameNightId });
    }

    // POST: /GameNights/Unregister/5
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unregister(int id)
    {
        var personId = await GetCurrentPersonIdAsync();
        if (personId == null)
        {
            TempData["Error"] = "Geen gekoppelde gebruiker gevonden.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var success = await _registrationService.UnregisterAsync(personId.Value, id);
        if (success)
        {
            TempData["Success"] = "Je bent uitgeschreven van deze spelavond.";
        }
        else
        {
            TempData["Error"] = "Er ging iets mis bij het uitschrijven.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<int?> GetCurrentPersonIdAsync()
    {
        // Try email claim first
        var email = User.FindFirstValue(ClaimTypes.Email);

        // If no email claim, try to get it from the Identity user
        ApplicationUser? identityUser = null;
        if (string.IsNullOrEmpty(email))
        {
            identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null) return null;
            if (identityUser.PersonId.HasValue) return identityUser.PersonId.Value;
            email = identityUser.Email;
        }

        if (string.IsNullOrEmpty(email)) return null;

        // Try to find domain person by email
        var person = await _personService.GetByEmailAsync(email);
        if (person != null) return person.Id;

        // No person exists: create a minimal Person and link it to the identity user
        var newPerson = new Person
        {
            FirstName = User.Identity?.Name ?? string.Empty,
            LastName = string.Empty,
            Email = email,
            DateOfBirth = DateTime.UtcNow.AddYears(-18),
            Gender = Gender.Other,
            Address = new Address()
        };

        var added = await _personRepository.AddAsync(newPerson);

        // Update identity user if available
        identityUser ??= await _userManager.GetUserAsync(User);
        if (identityUser != null)
        {
            identityUser.PersonId = added.Id;
            await _userManager.UpdateAsync(identityUser);
        }

        return added.Id;
    }
}
