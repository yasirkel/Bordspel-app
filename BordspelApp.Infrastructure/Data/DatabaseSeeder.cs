using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BordspelApp.Domain.Entities;
using BordspelApp.Domain.Enums;
using BordspelApp.Domain.ValueObjects;
using BordspelApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BordspelApp.Infrastructure.Data
{
    /// <summary>
    /// Development-only database seeder that creates Identity users, Persons, BoardGames and GameNights.
    /// Idempotent: will not duplicate records when re-run.
    /// Call from Program.cs using a scoped ServiceProvider: `await DatabaseSeeder.SeedAsync(scope.ServiceProvider);`
    /// </summary>
    public static class DatabaseSeeder
    {
        private const string DevPassword = "Test1234!"; // DEV-only password

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;

            var context = provider.GetRequiredService<BordspelAppDbContext>();
            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

            // Do not call MigrateAsync here - migrations should be applied separately.

            // Perform seeding in a transaction where possible
            await using var tx = await context.Database.BeginTransactionAsync();
            try
            {
                // Roles
                var roles = new[] { "Admin", "User" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // -- Persons + Identity users
                var seedPeople = GetSeedPeople();

                var createdPersons = new List<Person>();

                foreach (var p in seedPeople)
                {
                    // Ensure Person exists (unique by email)
                    var person = await context.Persons.FirstOrDefaultAsync(x => x.Email == p.Email);
                    if (person == null)
                    {
                        person = new Person
                        {
                            FirstName = p.FirstName,
                            LastName = p.LastName,
                            Email = p.Email,
                            DateOfBirth = p.DateOfBirth,
                            Gender = p.Gender,
                            Address = new Address(p.Street, p.HouseNumber, p.City, p.PostalCode),
                            DietaryRequirements = new DietaryRequirements(p.IsLactoseFree, p.IsNutFree, p.IsVegetarian, p.IsAlcoholFree)
                        };
                        context.Persons.Add(person);
                        await context.SaveChangesAsync();
                    }

                    createdPersons.Add(person);

                    // Ensure Identity user exists and is linked to person
                    var existingUser = await userManager.FindByEmailAsync(person.Email);
                    if (existingUser == null)
                    {
                        var appUser = new ApplicationUser
                        {
                            UserName = person.Email,
                            Email = person.Email,
                            PersonId = person.Id
                        };

                        var createResult = await userManager.CreateAsync(appUser, DevPassword);
                        if (!createResult.Succeeded)
                        {
                            // Try to continue but log to console - this is dev-only seeding
                            Console.WriteLine($"Failed to create user {person.Email}: {string.Join(';', createResult.Errors.Select(e=>e.Description))}");
                        }
                        else
                        {
                            // Default role assignment - first two users are admins
                            var roleToAssign = createdPersons.Count <= 2 ? "Admin" : "User";
                            await userManager.AddToRoleAsync(appUser, roleToAssign);
                        }
                    }
                    else
                    {
                        // Link existing Identity user to Person if missing
                        if (existingUser.PersonId == null || existingUser.PersonId == 0)
                        {
                            existingUser.PersonId = person.Id;
                            await userManager.UpdateAsync(existingUser);
                        }
                    }
                }

                // -- BoardGames
                var seedBoardGames = GetSeedBoardGames();
                foreach (var sb in seedBoardGames)
                {
                    if (!await context.BoardGames.AnyAsync(bg => bg.Name == sb.Name))
                    {
                        context.BoardGames.Add(new BoardGame
                        {
                            Name = sb.Name,
                            Description = sb.Description,
                            Genre = sb.Genre,
                            Type = sb.Type,
                            Is18Plus = sb.Is18Plus,
                            PhotoUrl = sb.PhotoUrl
                        });
                    }
                }
                await context.SaveChangesAsync();

                // -- GameNights
                {
                    var existingCount = await context.GameNights.CountAsync();
                    var toCreate = Math.Max(0, 6 - existingCount);
                    if (toCreate > 0)
                    {
                        var rnd = new Random(42);
                        var boardGames = await context.BoardGames.ToListAsync();

                        // Create required number of game nights to reach 6
                        for (int i = 0; i < toCreate; i++)
                        {
                            var organizer = createdPersons[rnd.Next(createdPersons.Count)];
                            var maxPlayers = rnd.Next(4, 9); // 4..8

                            var gn = new GameNight
                            {
                                Title = i == 0 && existingCount == 0 ? "Introductie Bordspellenavond" : $"Spellenavond #{existingCount + i + 1}",
                                Description = i == 0 && existingCount == 0 ? "Gezellige avond met verschillende spellen." : $"Een leuke spellenavond met vrienden (sessie {existingCount + i + 1}).",
                                DateTime = DateTime.UtcNow.AddDays(rnd.Next(-5, 15)),
                                MaxPlayers = maxPlayers,
                                OrganizerId = organizer.Id,
                                Address = new Address("Schoolstraat", (10 + i).ToString(), new[] { "Breda", "Eindhoven", "Rotterdam", "Amsterdam" }[(existingCount + i) % 4]),
                                HasLactoseFreeOption = rnd.Next(2) == 0,
                                HasNutFreeOption = rnd.Next(2) == 0,
                                HasVegetarianOption = rnd.Next(2) == 0,
                                HasAlcoholFreeOption = rnd.Next(2) == 0
                            };

                            context.GameNights.Add(gn);
                            await context.SaveChangesAsync();

                            // Attach 2-4 board games
                            var gamesToAttach = boardGames.OrderBy(x => rnd.Next()).Take(rnd.Next(2, 5)).ToList();
                            foreach (var bg in gamesToAttach)
                            {
                                if (!await context.GameNightBoardGames.AnyAsync(x => x.GameNightId == gn.Id && x.BoardGameId == bg.Id))
                                {
                                    context.GameNightBoardGames.Add(new GameNightBoardGame { GameNightId = gn.Id, BoardGameId = bg.Id });
                                }
                            }
                            await context.SaveChangesAsync();

                            // Recalculate 18+ status
                            gn.Recalculate18Plus();
                            await context.SaveChangesAsync();

                            // Add 3-6 players respecting 18+ if necessary
                            var candidatePlayers = createdPersons.OrderBy(x => rnd.Next()).ToList();
                            int playersNeeded = rnd.Next(3, Math.Min(6, createdPersons.Count) + 1);
                            int added = 0;
                            foreach (var person in candidatePlayers)
                            {
                                if (added >= playersNeeded) break;
                                if (gn.Is18Plus && !person.IsAtLeast18YearsOld()) continue;
                                if (!await context.GameNightPlayers.AnyAsync(x => x.GameNightId == gn.Id && x.PersonId == person.Id))
                                {
                                    context.GameNightPlayers.Add(new GameNightPlayer { GameNightId = gn.Id, PersonId = person.Id, RegisteredAt = DateTime.UtcNow });
                                    added++;
                                }
                            }
                            await context.SaveChangesAsync();
                        }
                    }
                }

                // Repair existing GameNights that may have too few players or board games
                var allGameNights = await context.GameNights.ToListAsync();
                var allBoardGames = await context.BoardGames.ToListAsync();
                var allPersons = await context.Persons.ToListAsync();
                var rndRepair = new Random(1337);

                foreach (var gn in allGameNights)
                {
                    var playerCount = await context.GameNightPlayers.CountAsync(x => x.GameNightId == gn.Id);
                    var bgCount = await context.GameNightBoardGames.CountAsync(x => x.GameNightId == gn.Id);

                    // Ensure at least 2 board games
                    while (bgCount < 2 && allBoardGames.Count > 0)
                    {
                        var candidate = allBoardGames.OrderBy(x => rndRepair.Next()).First();
                        if (!await context.GameNightBoardGames.AnyAsync(x => x.GameNightId == gn.Id && x.BoardGameId == candidate.Id))
                        {
                            context.GameNightBoardGames.Add(new GameNightBoardGame { GameNightId = gn.Id, BoardGameId = candidate.Id });
                            bgCount++;
                            await context.SaveChangesAsync();
                        }
                        else
                        {
                            break;
                        }
                    }

                    // Ensure at least 3 players (respect 18+)
                    var candidates = allPersons.OrderBy(x => rndRepair.Next()).ToList();
                    var added = 0;
                    foreach (var p in candidates)
                    {
                        if (playerCount + added >= 3) break;
                        if (gn.Is18Plus && !p.IsAtLeast18YearsOld()) continue;
                        if (!await context.GameNightPlayers.AnyAsync(x => x.GameNightId == gn.Id && x.PersonId == p.Id))
                        {
                            context.GameNightPlayers.Add(new GameNightPlayer { GameNightId = gn.Id, PersonId = p.Id, RegisteredAt = DateTime.UtcNow });
                            added++;
                        }
                    }

                    if (added > 0) await context.SaveChangesAsync();
                }

                await tx.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Seeding failed: {ex.Message}");
                await tx.RollbackAsync();
                throw;
            }
        }

        // -- Seed data helpers (small set of realistic sample data)
        private static List<(string FirstName, string LastName, string Email, DateTime DateOfBirth, Gender Gender, string Street, string HouseNumber, string City, string PostalCode, bool IsLactoseFree, bool IsNutFree, bool IsVegetarian, bool IsAlcoholFree)> GetSeedPeople()
        {
            var now = DateTime.UtcNow;
            return new List<(string, string, string, DateTime, Gender, string, string, string, string, bool, bool, bool, bool)>
            {
                ("Alice","De Wit","alice.dewit@example.com", new DateTime(1992,4,12), Gender.Female, "Lindenstraat","12","Breda","4811AA", false, false, false, false),
                ("Bob","Smit","bob.smit@example.com", new DateTime(1988,11,2), Gender.Male, "Hoofdstraat","5","Eindhoven","5611BB", false, true, false, true),
                ("Carol","Janssen","carol.janssen@example.com", new DateTime(2006,6,8), Gender.Female, "Stationsweg","3","Rotterdam","3011CC", true, false, true, false),
                ("David","van Dijk","david.vandijk@example.com", new DateTime(1999,1,20), Gender.Male, "Kerkplein","7","Amsterdam","1011DD", false, false, false, false),
                ("Eva","Kramer","eva.kramer@example.com", new DateTime(2004,9,30), Gender.Female, "Marktstraat","21","Breda","4812EE", true, true, false, false),
                ("Frank","Bakker","frank.bakker@example.com", new DateTime(1985,3,14), Gender.Male, "Noordlaan","18","Eindhoven","5612FF", false, false, false, true),
                ("Gina","Visser","gina.visser@example.com", new DateTime(2000,12,1), Gender.Female, "Oosterdijk","2","Rotterdam","3012GG", false, true, true, false),
                ("Henk","Meijer","henk.meijer@example.com", new DateTime(2002,7,23), Gender.Male, "Zuidstraat","9","Amsterdam","1012HH", false, false, false, false),
                ("Iris","Bos","iris.bos@example.com", new DateTime(1995,2,5), Gender.Female, "Lageweg","11","Breda","4813II", true, false, false, false),
                ("Jeroen","de Boer","jeroen.deboer@example.com", new DateTime(1998,5,17), Gender.Male, "Parklaan","4","Eindhoven","5613JJ", false, false, true, false)
            };
        }

        private static List<(string Name, string Description, GameGenre Genre, GameType Type, bool Is18Plus, string PhotoUrl)> GetSeedBoardGames()
        {
            return new List<(string, string, GameGenre, GameType, bool, string)>
            {
                ("Catan","Strategy to build settlements", GameGenre.Strategy, GameType.Board, false, "https://images.unsplash.com/photo-1545239351-1141bd82e8a6"),
                ("Pandemic","Cooperative world-saving game", GameGenre.Cooperative, GameType.Board, false, "https://images.unsplash.com/photo-1515879218367-8466d910aaa4"),
                ("Ganzenbord","Family classic", GameGenre.Family, GameType.Board, false, "https://images.unsplash.com/photo-1541534401786-6f9b3d3d8a3c"),
                ("Monopoly","Trade and manage property", GameGenre.Family, GameType.Board, false, "https://images.unsplash.com/photo-1542736667-069246bdbc3d"),
                ("Cards of Fate","Card game for adults", GameGenre.Card, GameType.Card, true, "https://images.unsplash.com/photo-1523528283118-2f8d1f3b4f7f"),
                ("Terraforming Mars","Sci-fi strategy", GameGenre.Strategy, GameType.Board, false, "https://images.unsplash.com/photo-1558981403-c9e0b78f4d6f")
            };
        }
    }
}
