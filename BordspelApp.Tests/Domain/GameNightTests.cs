using System;
using BordspelApp.Domain.Entities;
using BordspelApp.Tests.TestHelpers;
using Xunit;

namespace BordspelApp.Tests.Domain;

public class GameNightTests
{
    [Fact]
    // Controleert dat een volle avond geen nieuwe deelnemer accepteert.
    public void CanPersonJoin_GameNightIsFull_ReturnsFalse()
    {
        // Arrange
        var gn = Builders.CreateGameNight(maxPlayers: 1);
        gn.Players.Add(Builders.CreateRegistration(gn.Id, 2));
        var person = Builders.CreatePerson(id: 2, age: 20);

        // Act
        var canJoin = gn.CanPersonJoin(person);

        // Assert
        Assert.False(canJoin);
    }

    [Fact]
    // Controleert de minimumleeftijd van zestien jaar.
    public void CanPersonJoin_PersonUnder16_ReturnsFalse()
    {
        var gn = Builders.CreateGameNight(maxPlayers: 10);
        var person = Builders.CreatePerson(id: 3, age: 15);

        var canJoin = gn.CanPersonJoin(person);

        Assert.False(canJoin);
    }

    [Fact]
    // Controleert dat een minderjarige niet aan een 18+-avond kan deelnemen.
    public void CanPersonJoin_18PlusGame_PersonUnder18_ReturnsFalse()
    {
        var gn = Builders.CreateGameNight(maxPlayers: 10);
        // add an 18+ boardgame
        var bg = Builders.CreateBoardGame(is18Plus: true);
        gn.AddBoardGame(bg);
        gn.Recalculate18Plus();

        var person = Builders.CreatePerson(id: 4, age: 17);

        var canJoin = gn.CanPersonJoin(person);

        Assert.False(canJoin);
    }

    [Fact]
    // Controleert dat dubbele deelname wordt geweigerd.
    public void CanPersonJoin_AlreadyRegistered_ReturnsFalse()
    {
        var gn = Builders.CreateGameNight(maxPlayers: 10);
        var person = Builders.CreatePerson(id: 5, age: 25);
        gn.Players.Add(Builders.CreateRegistration(gn.Id, person.Id));

        var canJoin = gn.CanPersonJoin(person);

        Assert.False(canJoin);
    }

    [Fact]
    // Controleert de geldige inschrijving van een volwassen nieuwe deelnemer.
    public void CanPersonJoin_ValidPersonNotRegistered_ReturnsTrue()
    {
        var gn = Builders.CreateGameNight(maxPlayers: 10);
        var person = Builders.CreatePerson(id: 6, age: 20);

        var canJoin = gn.CanPersonJoin(person);

        Assert.True(canJoin);
    }

    [Fact]
    // Controleert dat een datum in het verleden ongeldig is.
    public void IsDateValid_PastDate_ReturnsFalse()
    {
        var gn = Builders.CreateGameNight(dateTime: DateTime.Now.AddMinutes(-5));
        Assert.False(gn.IsDateValid());
    }

    [Fact]
    // Controleert dat een toekomstige datum geldig is.
    public void IsDateValid_FutureDate_ReturnsTrue()
    {
        var gn = Builders.CreateGameNight(dateTime: DateTime.Now.AddDays(1));
        Assert.True(gn.IsDateValid());
    }

    [Fact]
    // Controleert dat één 18+-spel de hele avond als 18+ markeert.
    public void Recalculate18Plus_SetsIs18Plus_WhenAnyBoardGameIs18Plus()
    {
        var gn = Builders.CreateGameNight();
        gn.AddBoardGame(Builders.CreateBoardGame(id: 1, is18Plus: false));
        gn.AddBoardGame(Builders.CreateBoardGame(id: 2, is18Plus: true));

        gn.Recalculate18Plus();

        Assert.True(gn.Is18Plus);
    }

    [Fact]
    // Controleert dat een avond zonder 18+-spel niet beperkt wordt.
    public void Recalculate18Plus_SetsIs18Plus_False_WhenNoBoardGamesAre18Plus()
    {
        var gn = Builders.CreateGameNight();
        gn.AddBoardGame(Builders.CreateBoardGame(id: 3, is18Plus: false));

        gn.Recalculate18Plus();

        Assert.False(gn.Is18Plus);
    }

    [Fact]
    // Controleert dat een volwassen organisator wordt geaccepteerd.
    public void IsOrganizerValid_ReturnsTrue_WhenOrganizerIs18Plus()
    {
        var organizer = Builders.CreatePerson(id: 10, age: 30);
        var gn = Builders.CreateGameNight(organizerId: organizer.Id);
        gn.Organizer = organizer;

        Assert.True(gn.IsOrganizerValid());
    }
}
