using BordspelApp.Application.Interfaces;
using BordspelApp.Application.Models;
using BordspelApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BordspelApp.Infrastructure.Services;

public class RegistrationService : IRegistrationService
{
    private readonly IGameNightRepository _gameNightRepository;
    private readonly IPersonRepository _personRepository;

    public RegistrationService(IGameNightRepository gameNightRepository, IPersonRepository personRepository)
    {
        _gameNightRepository = gameNightRepository;
        _personRepository = personRepository;
    }

    /// <summary>Controleert de inschrijfregels en registreert de persoon als dat is toegestaan.</summary>
    public async Task<RegistrationResponse> RegisterAsync(int personId, int gameNightId, bool forceRegister = false)
    {
        var person = await _personRepository.GetAllQueryable()
            .Include(p => p.GameNightParticipations)
                .ThenInclude(gnp => gnp.GameNight)
            .FirstOrDefaultAsync(p => p.Id == personId);

        if (person == null)
        {
            return new RegistrationResponse
            {
                Result = RegistrationResult.PersonNotFound,
                Message = "Gebruiker niet gevonden."
            };
        }

        var gameNight = await _gameNightRepository.GetAllQueryable()
            .Include(gn => gn.Players)
            .Include(gn => gn.BoardGames)
                .ThenInclude(gnbg => gnbg.BoardGame)
            .FirstOrDefaultAsync(gn => gn.Id == gameNightId);

        if (gameNight == null)
        {
            return new RegistrationResponse
            {
                Result = RegistrationResult.GameNightNotFound,
                Message = "Spelavond niet gevonden."
            };
        }

        // Controleer of de persoon al aan deze avond deelneemt.
        if (gameNight.Players.Any(p => p.PersonId == personId))
        {
            return new RegistrationResponse
            {
                Result = RegistrationResult.AlreadyRegistered,
                Message = "Je bent al ingeschreven voor deze spelavond."
            };
        }

        // Controleer of de capaciteit van de avond is bereikt.
        if (gameNight.Players.Count >= gameNight.MaxPlayers)
        {
            return new RegistrationResponse
            {
                Result = RegistrationResult.GameNightFull,
                Message = "Deze spelavond is vol."
            };
        }

        // Controleer de 18+-beperking van de avond.
        var age = CalculateAge(person.DateOfBirth);
        if (gameNight.Is18Plus && age < 18)
        {
            return new RegistrationResponse
            {
                Result = RegistrationResult.AgeRestriction,
                Message = "Deze spelavond is alleen toegankelijk voor 18+."
            };
        }

        // Controleer de regel van maximaal één spelavond per dag.
        var gameNightDate = DateOnly.FromDateTime(gameNight.DateTime);

        // Laad de deelnames opnieuw zodat de datum van iedere spelavond beschikbaar is.
        var participatingNights = await _gameNightRepository.GetGameNightsByParticipantAsync(personId);
        var existingRegistrationSameDay = participatingNights
            .Any(gn => gn.Id != gameNightId && DateOnly.FromDateTime(gn.DateTime) == gameNightDate);

        if (existingRegistrationSameDay)
        {
            return new RegistrationResponse
            {
                Result = RegistrationResult.AlreadyRegisteredSameDay,
                Message = "Je bent al ingeschreven voor een andere spelavond op dezelfde dag."
            };
        }

        // Verzamel waarschuwingen wanneer profielvoorkeuren niet worden aangeboden.
        var dietaryWarnings = new List<string>();
        
        if (person.DietaryRequirements.IsLactoseFree && !gameNight.HasLactoseFreeOption)
        {
            dietaryWarnings.Add("Deze spelavond heeft geen lactosevrije opties, maar jouw profiel geeft 'lactosevrij' aan.");
        }
        
        if (person.DietaryRequirements.IsNutFree && !gameNight.HasNutFreeOption)
        {
            dietaryWarnings.Add("Deze spelavond heeft geen notenvrije opties, maar jouw profiel geeft 'notenvrij' aan.");
        }
        
        if (person.DietaryRequirements.IsVegetarian && !gameNight.HasVegetarianOption)
        {
            dietaryWarnings.Add("Deze spelavond heeft geen vegetarische opties, maar jouw profiel geeft 'vegetarisch' aan.");
        }
        
        if (person.DietaryRequirements.IsAlcoholFree && !gameNight.HasAlcoholFreeOption)
        {
            dietaryWarnings.Add("Deze spelavond heeft geen alcoholvrije opties, maar jouw profiel geeft 'geen alcohol' aan.");
        }

        // Geef eerst een waarschuwing; forceRegister bevestigt de keuze expliciet.
        if (dietaryWarnings.Any() && !forceRegister)
        {
            return new RegistrationResponse
            {
                Result = RegistrationResult.DietaryMismatch,
                Message = "Let op: er zijn enkele dieetwensen die niet overeenkomen met deze spelavond.",
                DietaryWarnings = dietaryWarnings
            };
        }

        // Voeg de registratie toe nadat alle regels zijn doorlopen.
        var registration = new GameNightPlayer(gameNightId, personId);
        gameNight.Players.Add(registration);
        
        await _gameNightRepository.UpdateAsync(gameNight);

        return new RegistrationResponse
        {
            Result = RegistrationResult.Success,
            Message = "Je bent succesvol ingeschreven voor deze spelavond!"
        };
    }

    /// <summary>Verwijdert de eigen deelname aan een spelavond.</summary>
    public async Task<bool> UnregisterAsync(int personId, int gameNightId)
    {
        var gameNight = await _gameNightRepository.GetAllQueryable()
            .Include(gn => gn.Players)
            .FirstOrDefaultAsync(gn => gn.Id == gameNightId);

        if (gameNight == null) return false;

        var registration = gameNight.Players.FirstOrDefault(p => p.PersonId == personId);
        if (registration == null) return false;

        gameNight.Players.Remove(registration);
        await _gameNightRepository.UpdateAsync(gameNight);

        return true;
    }

    /// <summary>Controleert of een persoon bij een spelavond staat ingeschreven.</summary>
    public async Task<bool> IsRegisteredAsync(int personId, int gameNightId)
    {
        return await _gameNightRepository.GetAllQueryable()
            .AsNoTracking()
            .AnyAsync(gn => gn.Id == gameNightId && gn.Players.Any(p => p.PersonId == personId));
    }

    // Berekent een leeftijd waarbij een verjaardag later in het jaar correct wordt verwerkt.
    private int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}
