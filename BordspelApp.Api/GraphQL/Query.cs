using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using BordspelApp.Application.Interfaces;
using BordspelApp.Api.GraphQL.Models;
using BordspelApp.Api.Services;
using BordspelApp.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BordspelApp.Api.GraphQL;

public class Query
{
    // Leest alle spelavonden met hun organisator en bordspellen uit de domeindatabase.
    public async Task<IEnumerable<GameNightDto>> GetGameNights([Service] IDbContextFactory<BordspelAppDbContext> contextFactory)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var gameNights = await context.GameNights
            .AsNoTracking()
            .Include(gn => gn.Organizer)
            .Include(gn => gn.BoardGames).ThenInclude(bg => bg.BoardGame)
            .ToListAsync();

        return gameNights.Select(gn => new GameNightDto
        {
            Id = gn.Id,
            DateTime = gn.DateTime,
            Title = gn.Title,
            OrganizerName = gn.Organizer?.FullName,
            BoardGames = gn.BoardGames
                .Where(b => b.BoardGame != null)
                .Select(b => new BoardGameDto
                {
                    Id = b.BoardGame!.Id,
                    Name = b.BoardGame!.Name,
                    Genre = b.BoardGame!.Genre.ToString()
                })
                .ToList()
        });
    }

    // Leest alle bordspellen uit de domeindatabase voor de GraphQL-query.
    public async Task<IEnumerable<BoardGameDto>> GetBoardGames([Service] IDbContextFactory<BordspelAppDbContext> contextFactory)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var games = await context.BoardGames.AsNoTracking().ToListAsync();
        return games.Select(game => new BoardGameDto
        {
            Id = game.Id,
            Name = game.Name,
            Genre = game.Genre.ToString()
        });
    }

    // Recommend game nights to a person based on overlap with their co-players.
    // Geeft toekomstige avonden aan die aansluiten bij eerdere medespelers.
    public async Task<IEnumerable<RecommendationDto>> GetRecommendedGameNights(
        [Service] IDbContextFactory<BordspelAppDbContext> contextFactory,
        [Service] CurrentUserAccessor currentUser,
        [Service] IHttpContextAccessor httpContextAccessor,
        int limit = 6)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
        {
            throw new GraphQLException("Authentication is required for recommendations.");
        }

        var personId = await currentUser.GetPersonIdAsync(httpContextAccessor.HttpContext.User);
        if (personId is null) throw new GraphQLException("The authenticated user is not linked to a person.");
        limit = Math.Clamp(limit, 1, 25);
        await using var context = await contextFactory.CreateDbContextAsync();
        var all = (await context.GameNights
            .AsNoTracking()
            .Include(gn => gn.Organizer)
            .Include(gn => gn.Players)
            .Include(gn => gn.BoardGames).ThenInclude(bg => bg.BoardGame)
            .ToListAsync())
            .Where(gn => gn.DateTime > DateTime.Now)
            .ToList();

        // Bepaal de avonden waaraan de persoon al deelneemt.
        var myNights = all.Where(gn => gn.Players.Any(p => p.PersonId == personId)).ToList();
        var myNightIds = myNights.Select(gn => gn.Id).ToHashSet();

        // Verzamel unieke medespelers uit die avonden.
        var coPlayers = myNights
            .SelectMany(gn => gn.Players)
            .Where(p => p.PersonId != personId)
            .Select(p => p.PersonId)
            .Distinct()
            .ToHashSet();

        // Filter toekomstige avonden waarin de persoon nog niet zit.
        var candidates = all.Where(gn => !myNightIds.Contains(gn.Id)).ToList();

        var scored = candidates
            .Select(gn => new RecommendationDto
            {
                GameNight = new GameNightDto
                {
                    Id = gn.Id,
                    DateTime = gn.DateTime,
                    Title = gn.Title,
                    OrganizerName = gn.Organizer?.FullName,
                    BoardGames = gn.BoardGames.Where(b => b.BoardGame != null)
                        .Select(b => new BoardGameDto
                        {
                            Id = b.BoardGame!.Id,
                            Name = b.BoardGame!.Name,
                            Genre = b.BoardGame!.Genre.ToString()
                        }).ToList()
                },
                CommonPlayersCount = gn.Players.Select(p => p.PersonId).Distinct().Intersect(coPlayers).Count()
            })
            .Where(r => r.CommonPlayersCount > 0)
            .OrderByDescending(r => r.CommonPlayersCount)
            .ThenBy(r => r.GameNight!.DateTime)
            .Take(limit)
            .ToList();

        return scored;
    }
}
