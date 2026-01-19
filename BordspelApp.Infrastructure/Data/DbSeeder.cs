using BordspelApp.Domain.Entities;
using BordspelApp.Domain.Enums;
using BordspelApp.Domain.ValueObjects;

namespace BordspelApp.Infrastructure.Data;

public static class DbSeeder
{
    public static void Seed(BordspelAppDbContext context)
    {
        // Check if already seeded
        if (context.Persons.Any())
        {
            return;
        }

        // Create Persons
        var organizer = new Person
        {
            FirstName = "Jan",
            LastName = "de Vries",
            Email = "jan.devries@example.com",
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = Gender.Male,
            Address = new Address("Lovensdijkstraat", "61", "Breda"),
            DietaryRequirements = new DietaryRequirements(false, false, false, false)
        };

        var player = new Person
        {
            FirstName = "Anna",
            LastName = "Jansen",
            Email = "anna.jansen@example.com",
            DateOfBirth = new DateTime(2000, 8, 20),
            Gender = Gender.Female,
            Address = new Address("Hogeschoollaan", "1", "Breda"),
            DietaryRequirements = new DietaryRequirements(true, true, true, false)
        };

        var yasir = new Person
        {
            FirstName = "Yasir",
            LastName = "Kel",
            Email = "yasir@email.com",
            DateOfBirth = new DateTime(1999, 3, 10),
            Gender = Gender.Male,
            Address = new Address("Studentenstraat", "42", "4818AB", "Breda"),
            DietaryRequirements = new DietaryRequirements(false, false, false, false)
        };

        context.Persons.AddRange(organizer, player, yasir);
        context.SaveChanges();

        // Create Board Games
        var catan = new BoardGame
        {
            Name = "Catan",
            Description = "Een strategisch bordspel over het bouwen van nederzettingen",
            Genre = GameGenre.Strategy,
            Type = GameType.Board,
            Is18Plus = false,
            PhotoUrl = "https://example.com/catan.jpg"
        };

        var poker = new BoardGame
        {
            Name = "Poker",
            Description = "Klassiek kaartspel",
            Genre = GameGenre.Card,
            Type = GameType.Card,
            Is18Plus = true,
            PhotoUrl = "https://example.com/poker.jpg"
        };

        var pandemic = new BoardGame
        {
            Name = "Pandemic",
            Description = "Coöperatief spel om de wereld te redden van ziektes",
            Genre = GameGenre.Cooperative,
            Type = GameType.Board,
            Is18Plus = false,
            PhotoUrl = "https://example.com/pandemic.jpg"
        };

        var ganzenbord = new BoardGame
        {
            Name = "Ganzenbord",
            Description = "Een klassiek familiespel",
            Genre = GameGenre.Family,
            Type = GameType.Board,
            Is18Plus = false,
            PhotoUrl = "https://example.com/ganzenbord.jpg"
        };

        context.BoardGames.AddRange(catan, poker, pandemic);
        context.SaveChanges();

        // Create Game Night
        var gameNight = new GameNight
        {
            DateTime = DateTime.Now.AddDays(7),
            MaxPlayers = 6,
            OrganizerId = organizer.Id,
            Address = new Address("Lovensdijkstraat", "61", "Breda"),
            HasLactoseFreeOption = true,
            HasNutFreeOption = true,
            HasVegetarianOption = true,
            HasAlcoholFreeOption = false
        };

        context.GameNights.Add(gameNight);
        context.SaveChanges();

        // Add board games to game night
        var gameNightCatan = new GameNightBoardGame
        {
            GameNightId = gameNight.Id,
            BoardGameId = catan.Id
        };

        var gameNightPoker = new GameNightBoardGame
        {
            GameNightId = gameNight.Id,
            BoardGameId = poker.Id
        };

        context.GameNightBoardGames.AddRange(gameNightCatan, gameNightPoker);
        context.SaveChanges();

        // Recalculate 18+ status
        gameNight.Recalculate18Plus();
        context.SaveChanges();

        // Add player to game night
        var gameNightPlayer = new GameNightPlayer
        {
            GameNightId = gameNight.Id,
            PersonId = player.Id,
            RegisteredAt = DateTime.UtcNow
        };

        context.GameNightPlayers.Add(gameNightPlayer);
        context.SaveChanges();
    }
}
