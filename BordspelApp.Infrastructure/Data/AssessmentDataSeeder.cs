using BordspelApp.Domain.Entities;
using BordspelApp.Domain.Enums;
using BordspelApp.Domain.ValueObjects;
using BordspelApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BordspelApp.Infrastructure.Data;

/// <summary>
/// Expliciete, idempotente seed voor de gedeelde demonstratieomgeving.
/// Deze seed wordt bewust niet vanuit WebApp- of API-startup uitgevoerd.
/// </summary>
public static class AssessmentDataSeeder
{
    private sealed record PersonSeed(
        string Email,
        string FirstName,
        string LastName,
        DateTime DateOfBirth,
        Gender Gender,
        string Street,
        string HouseNumber,
        string City,
        string PostalCode,
        bool LactoseFree,
        bool NutFree,
        bool Vegetarian,
        bool AlcoholFree,
        string Role,
        string PasswordVariable);

    private sealed record BoardGameSeed(
        string Name,
        string Description,
        GameGenre Genre,
        GameType Type,
        bool Is18Plus,
        string PhotoUrl);

    public static async Task SeedAsync(
        BordspelAppDbContext domain,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        CancellationToken cancellationToken = default)
    {
        foreach (var role in new[] { "Admin", "client" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                EnsureSucceeded(result, $"creating role {role}");
            }
        }

        var people = new Dictionary<string, Person>();
        foreach (var seed in GetPeople())
        {
            var person = await domain.Persons.SingleOrDefaultAsync(x => x.Email == seed.Email, cancellationToken);
            if (person is null)
            {
                person = new Person
                {
                    Email = seed.Email,
                    FirstName = seed.FirstName,
                    LastName = seed.LastName,
                    DateOfBirth = seed.DateOfBirth,
                    Gender = seed.Gender,
                    Address = new Address(seed.Street, seed.HouseNumber, seed.City, seed.PostalCode),
                    DietaryRequirements = new DietaryRequirements(seed.LactoseFree, seed.NutFree, seed.Vegetarian, seed.AlcoholFree)
                };
                domain.Persons.Add(person);
                await domain.SaveChangesAsync(cancellationToken);
            }

            people[seed.Email] = person;
            await EnsureIdentityUserAsync(userManager, seed, person, cancellationToken);
        }

        var games = new Dictionary<string, BoardGame>();
        foreach (var seed in GetBoardGames())
        {
            var game = await domain.BoardGames.SingleOrDefaultAsync(x => x.Name == seed.Name, cancellationToken);
            if (game is null)
            {
                game = new BoardGame
                {
                    Name = seed.Name,
                    Description = seed.Description,
                    Genre = seed.Genre,
                    Type = seed.Type,
                    Is18Plus = seed.Is18Plus,
                    PhotoUrl = seed.PhotoUrl
                };
                domain.BoardGames.Add(game);
                await domain.SaveChangesAsync(cancellationToken);
            }
            else if (!string.Equals(game.PhotoUrl, seed.PhotoUrl, StringComparison.Ordinal))
            {
                // Keep existing board games and relationships, but repair their image URL.
                game.PhotoUrl = seed.PhotoUrl;
                await domain.SaveChangesAsync(cancellationToken);
            }

            games[seed.Name] = game;
        }

        var organizer = people["assessment.organizer@example.com"];
        var docent = people["assessment.docent@example.com"];
        var playerOne = people["assessment.player1@example.com"];
        var playerTwo = people["assessment.player2@example.com"];
        var playerThree = people["assessment.player3@example.com"];
        var minor = people["assessment.minor@example.com"];

        var nights = new Dictionary<string, GameNight>
        {
            ["Taal & Tactiek"] = await EnsureGameNightAsync(domain, "Taal & Tactiek", "Normale avond met strategie en samenwerking.", organizer, DateTime.UtcNow.AddDays(7), 6, false, new[] { "Catan", "Azul" }, new[] { playerOne, playerTwo }, games, cancellationToken),
            ["Nacht van de Kaarten"] = await EnsureGameNightAsync(domain, "Nacht van de Kaarten", "Avond met een volwassen kaartspel.", organizer, DateTime.UtcNow.AddDays(10), 6, false, new[] { "Cards of Fate" }, new[] { playerOne, playerTwo }, games, cancellationToken),
            ["Volle Tafel"] = await EnsureGameNightAsync(domain, "Volle Tafel", "Demonstratie van de capaciteitsregel.", organizer, DateTime.UtcNow.AddDays(12), 3, false, new[] { "Pandemic", "Ticket to Ride" }, new[] { playerOne, playerTwo, playerThree }, games, cancellationToken),
            ["Internationale Potluck"] = await EnsureGameNightAsync(domain, "Internationale Potluck", "Iedereen neemt iets mee dat bij verschillende dieetwensen past.", organizer, DateTime.UtcNow.AddDays(14), 8, true, new[] { "Catan", "Dixit", "Pandemic" }, new[] { playerOne, playerTwo, playerThree }, games, cancellationToken),
            ["Vrije Speelavond"] = await EnsureGameNightAsync(domain, "Vrije Speelavond", "Lege avond die de organisator nog kan wijzigen of verwijderen.", organizer, DateTime.UtcNow.AddDays(18), 6, false, new[] { "Azul", "Dixit" }, Array.Empty<Person>(), games, cancellationToken)
        };

        await AddPotluckItemAsync(domain, nights["Internationale Potluck"], playerOne, "Hummus en groentesticks", true, true, true, true, cancellationToken);
        await AddPotluckItemAsync(domain, nights["Internationale Potluck"], playerTwo, "Lactosevrije fruitcake", true, false, true, true, cancellationToken);
        await AddPotluckItemAsync(domain, nights["Internationale Potluck"], playerThree, "Notenvrije kaasplank", false, true, false, true, cancellationToken);

        _ = docent;
        _ = minor;
    }

    private static async Task EnsureIdentityUserAsync(
        UserManager<ApplicationUser> userManager,
        PersonSeed seed,
        Person person,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(seed.Email);
        if (user is null)
        {
            var password = Environment.GetEnvironmentVariable(seed.PasswordVariable);
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException($"Omgevingsvariabele {seed.PasswordVariable} is nodig om een nieuw demonstratieaccount aan te maken.");
            }

            user = new ApplicationUser { UserName = seed.Email, Email = seed.Email, PersonId = person.Id };
            var result = await userManager.CreateAsync(user, password);
            EnsureSucceeded(result, $"creating user {seed.Email}");
        }
        else if (user.PersonId != person.Id)
        {
            user.PersonId = person.Id;
            var result = await userManager.UpdateAsync(user);
            EnsureSucceeded(result, $"linking user {seed.Email}");
        }

        if (!await userManager.IsInRoleAsync(user, seed.Role))
        {
            var result = await userManager.AddToRoleAsync(user, seed.Role);
            EnsureSucceeded(result, $"assigning role {seed.Role} to {seed.Email}");
        }
    }

    private static async Task<GameNight> EnsureGameNightAsync(
        BordspelAppDbContext domain,
        string title,
        string description,
        Person organizer,
        DateTime dateTime,
        int maxPlayers,
        bool isPotluck,
        IReadOnlyCollection<string> gameNames,
        IReadOnlyCollection<Person> players,
        IReadOnlyDictionary<string, BoardGame> games,
        CancellationToken cancellationToken)
    {
        var night = await domain.GameNights
            .Include(x => x.BoardGames)
            .ThenInclude(x => x.BoardGame)
            .Include(x => x.Players)
            .SingleOrDefaultAsync(x => x.Title == title && x.OrganizerId == organizer.Id, cancellationToken);

        if (night is null)
        {
            night = new GameNight(title, description, dateTime, new Address("Spelplein", "10", "Breda", "4811AA"), maxPlayers, organizer.Id)
            {
                IsPotluck = isPotluck,
                HasLactoseFreeOption = true,
                HasNutFreeOption = true,
                HasVegetarianOption = true,
                HasAlcoholFreeOption = true
            };
            domain.GameNights.Add(night);
            await domain.SaveChangesAsync(cancellationToken);
        }
        else
        {
            // Houd bestaande demonstratieavonden herkenbaar zonder hun relaties te wijzigen.
            night.Address.UpdateAddress("Spelplein", "10", "Breda", "4811AA");
        }

        foreach (var gameName in gameNames)
        {
            var game = games[gameName];
            if (!night.BoardGames.Any(x => x.BoardGameId == game.Id))
            {
                night.BoardGames.Add(new GameNightBoardGame { GameNightId = night.Id, BoardGameId = game.Id, BoardGame = game });
            }
        }

        foreach (var player in players)
        {
            if (!night.Players.Any(x => x.PersonId == player.Id) && night.Players.Count < night.MaxPlayers)
            {
                if (!night.Is18Plus || player.IsAtLeast18YearsOld())
                {
                    night.Players.Add(new GameNightPlayer(night.Id, player.Id));
                }
            }
        }

        night.IsPotluck = isPotluck;
        night.Recalculate18Plus();
        await domain.SaveChangesAsync(cancellationToken);
        return night;
    }

    private static async Task AddPotluckItemAsync(
        BordspelAppDbContext domain,
        GameNight night,
        Person person,
        string description,
        bool lactoseFree,
        bool nutFree,
        bool vegetarian,
        bool alcoholFree,
        CancellationToken cancellationToken)
    {
        var exists = await domain.PotluckItems.AnyAsync(x => x.GameNightId == night.Id && x.PersonId == person.Id && x.Description == description, cancellationToken);
        if (!exists)
        {
            domain.PotluckItems.Add(new PotluckItem(night.Id, person.Id, description, lactoseFree, nutFree, vegetarian, alcoholFree));
            await domain.SaveChangesAsync(cancellationToken);
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed while {operation}: {string.Join(';', result.Errors.Select(x => x.Description))}");
        }
    }

    private static IReadOnlyCollection<PersonSeed> GetPeople() => new[]
    {
        new PersonSeed("assessment.docent@example.com", "Marcel", "Vermeer", new DateTime(1978, 4, 12), Gender.Male, "Docentlaan", "1", "Breda", "4811AA", false, false, false, false, "Admin", "ASSESSMENT_DOCENT_PASSWORD"),
        new PersonSeed("assessment.organizer@example.com", "Sanne", "Jansen", new DateTime(1988, 6, 21), Gender.Female, "Spelstraat", "2", "Breda", "4811AB", false, false, false, false, "client", "ASSESSMENT_ORGANIZER_PASSWORD"),
        new PersonSeed("assessment.player1@example.com", "Noah", "Bakker", new DateTime(1995, 2, 3), Gender.Other, "Kastanjelaan", "3", "Breda", "4811AC", true, false, false, true, "client", "ASSESSMENT_PLAYER1_PASSWORD"),
        new PersonSeed("assessment.player2@example.com", "Amina", "El Idrissi", new DateTime(1992, 9, 17), Gender.Female, "Parkweg", "4", "Breda", "4811AD", false, true, true, false, "client", "ASSESSMENT_PLAYER2_PASSWORD"),
        new PersonSeed("assessment.player3@example.com", "Daan", "Smit", new DateTime(1990, 11, 8), Gender.Male, "Marktplein", "5", "Breda", "4811AE", false, false, false, true, "client", "ASSESSMENT_PLAYER3_PASSWORD"),
        new PersonSeed("assessment.minor@example.com", "Lina", "de Boer", new DateTime(2009, 3, 14), Gender.Female, "Schoolweg", "6", "Breda", "4811AF", true, true, true, true, "client", "ASSESSMENT_MINOR_PASSWORD")
    };

    private static IReadOnlyCollection<BoardGameSeed> GetBoardGames() => new[]
    {
        new BoardGameSeed("Catan", "Strategy to build settlements", GameGenre.Strategy, GameType.Board, false, "/images/boardgames/catan.svg"),
        new BoardGameSeed("Pandemic", "Cooperative world-saving game", GameGenre.Cooperative, GameType.Board, false, "/images/boardgames/pandemic.svg"),
        new BoardGameSeed("Cards of Fate", "Card game for adults", GameGenre.Card, GameType.Card, true, "/images/boardgames/cards-of-fate.svg"),
        new BoardGameSeed("Azul", "Tactical tile drafting with colorful patterns", GameGenre.Strategy, GameType.Tile, false, "/images/boardgames/azul.svg"),
        new BoardGameSeed("Ticket to Ride", "Build railway routes across a changing map", GameGenre.Family, GameType.Board, false, "/images/boardgames/ticket-to-ride.svg"),
        new BoardGameSeed("Dixit", "A creative storytelling card game", GameGenre.Party, GameType.Card, false, "/images/boardgames/dixit.svg")
    };
}
