using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using BordspelApp.Application.Interfaces;
using BordspelApp.Api.GraphQL.Models;

namespace BordspelApp.Api.GraphQL;

public class Query
{
    // Returns DTOs for all game nights with related board games. Uses repository
    // GetAllWithDetailsAsync() which eagerly loads related navigations.
    public async Task<IEnumerable<GameNightDto>> GetGameNights([Service] IGameNightRepository repo)
    {
        var gameNights = await repo.GetAllWithDetailsAsync();

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

    // Recommend game nights to a person based on overlap with their co-players.
    public async Task<IEnumerable<RecommendationDto>> GetRecommendedGameNights([Service] IGameNightRepository repo, int personId, int limit = 6)
    {
        var all = (await repo.GetAllWithDetailsAsync()).Where(gn => gn.DateTime > DateTime.Now).ToList();

        // Nights the person already participates in
        var myNights = all.Where(gn => gn.Players.Any(p => p.PersonId == personId)).ToList();
        var myNightIds = myNights.Select(gn => gn.Id).ToHashSet();

        // Co-players (people who attend nights the person attends)
        var coPlayers = myNights
            .SelectMany(gn => gn.Players)
            .Where(p => p.PersonId != personId)
            .Select(p => p.PersonId)
            .Distinct()
            .ToHashSet();

        // Candidate nights: upcoming nights person isn't participating in
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
