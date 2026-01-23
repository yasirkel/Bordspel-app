using System;
using BordspelApp.Domain.Entities;

namespace BordspelApp.Tests.TestHelpers;

public static class Builders
{
    public static Person CreatePerson(int id = 1, int age = 20)
    {
        var dob = DateTime.Today.AddYears(-age);
        return new Person
        {
            Id = id,
            FirstName = "Test",
            LastName = "User",
            Email = $"test{ id }@example.com",
            DateOfBirth = dob
        };
    }

    public static BoardGame CreateBoardGame(int id = 1, bool is18Plus = false)
    {
        return new BoardGame
        {
            Id = id,
            Name = "BG",
            Is18Plus = is18Plus
        };
    }

    public static GameNight CreateGameNight(int id = 1, int maxPlayers = 10, DateTime? dateTime = null, int organizerId = 1)
    {
        var gn = new GameNight("Title", null, dateTime ?? DateTime.Now.AddDays(1), new BordspelApp.Domain.ValueObjects.Address(), maxPlayers, organizerId)
        {
            Id = id
        };
        return gn;
    }

    public static GameNightPlayer CreateRegistration(int gameNightId, int personId)
    {
        return new GameNightPlayer(gameNightId, personId);
    }
}
