using BordspelApp.Domain.Entities;
using BordspelApp.Domain.Enums;
using BordspelApp.Domain.ValueObjects;

namespace BordspelApp.Infrastructure.Data;

public static class DbSeeder
{
    public static void Seed(BordspelAppDbContext context)
    {
        // Ensure persons exist
        if (!context.Persons.Any())
        {
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
        }

        // Ensure board games exist (add missing ones)
        var requiredGames = new[]
        {
            new { Name = "Catan", Description = "Een strategisch bordspel over het bouwen van nederzettingen", Genre = GameGenre.Strategy, Type = GameType.Board, Is18 = false, Url = "https://example.com/catan.jpg" },
            new { Name = "Poker", Description = "Klassiek kaartspel", Genre = GameGenre.Card, Type = GameType.Card, Is18 = true, Url = "https://example.com/poker.jpg" },
            new { Name = "Pandemic", Description = "Coöperatief spel om de wereld te redden van ziektes", Genre = GameGenre.Cooperative, Type = GameType.Board, Is18 = false, Url = "https://example.com/pandemic.jpg" },
            new { Name = "Ganzenbord", Description = "Een klassiek familiespel", Genre = GameGenre.Family, Type = GameType.Board, Is18 = false, Url = "https://example.com/ganzenbord.jpg" },
            new { Name = "Domino", Description = "Een klassiek tegelspel", Genre = GameGenre.Family, Type = GameType.Board, Is18 = false, Url = "https://example.com/domino.jpg" },
            new { Name = "Uno", Description = "Populair kaartspel voor alle leeftijden", Genre = GameGenre.Family, Type = GameType.Card, Is18 = false, Url = "https://example.com/uno.jpg" },
            new { Name = "Monopoly", Description = "Klassiek handelsspel", Genre = GameGenre.Family, Type = GameType.Board, Is18 = false, Url = "https://example.com/monopoly.jpg" }
        };

        foreach (var g in requiredGames)
        {
            if (!context.BoardGames.Any(b => b.Name == g.Name))
            {
                var bg = new BoardGame
                {
                    Name = g.Name,
                    Description = g.Description,
                    Genre = g.Genre,
                    Type = g.Type,
                    Is18Plus = g.Is18,
                    PhotoUrl = g.Url
                };
                context.BoardGames.Add(bg);
            }
        }

        context.SaveChanges();

        // Ensure there is at least one game night
        if (!context.GameNights.Any())
        {
            var organizer = context.Persons.FirstOrDefault(p => p.Email == "jan.devries@example.com") ?? context.Persons.First();

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

            // Link first two available board games to the game night
            var firstGames = context.BoardGames.Take(2).ToList();
            foreach (var bg in firstGames)
            {
                if (!context.GameNightBoardGames.Any(x => x.GameNightId == gameNight.Id && x.BoardGameId == bg.Id))
                {
                    context.GameNightBoardGames.Add(new GameNightBoardGame { GameNightId = gameNight.Id, BoardGameId = bg.Id });
                }
            }
            context.SaveChanges();

            // Recalculate 18+ status and add a sample player
            gameNight.Recalculate18Plus();
            context.SaveChanges();

            var playerPerson = context.Persons.FirstOrDefault(p => p.Email == "anna.jansen@example.com") ?? context.Persons.Skip(1).FirstOrDefault();
            if (playerPerson != null && !context.GameNightPlayers.Any(x => x.GameNightId == gameNight.Id && x.PersonId == playerPerson.Id))
            {
                context.GameNightPlayers.Add(new GameNightPlayer { GameNightId = gameNight.Id, PersonId = playerPerson.Id, RegisteredAt = DateTime.UtcNow });
                context.SaveChanges();
            }
        }
    }
}
