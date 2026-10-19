using BordspelApp.Application.DTOs;
using BordspelApp.Domain.Entities;
using BordspelApp.Infrastructure.Repositories;
using BordspelApp.Infrastructure.Services;
using BordspelApp.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BordspelApp.Tests.Application;

public class PotluckServiceTests
{
    [Fact]
    public async Task AddItemAsync_RegisteredParticipant_AddsItem()
    {
        var repository = new Mock<BordspelApp.Application.Interfaces.IPotluckRepository>();
        repository.Setup(x => x.GameNightExistsAsync(10)).ReturnsAsync(true);
        repository.Setup(x => x.IsParticipantAsync(10, 20)).ReturnsAsync(true);
        repository.Setup(x => x.AddAsync(It.IsAny<PotluckItem>()))
            .ReturnsAsync((PotluckItem item) => item);

        var service = new PotluckService(repository.Object);

        var result = await service.AddItemAsync(new PotluckItemCreateDto
        {
            GameNightId = 10,
            PersonId = 20,
            Description = "Appeltaart",
            IsLactoseFree = false,
            IsNutFree = true,
            IsVegetarian = true,
            IsAlcoholFree = true
        });

        Assert.NotNull(result);
        Assert.Equal("Appeltaart", result.Description);
        Assert.True(result.IsNutFree);
        Assert.True(result.IsVegetarian);
        repository.Verify(x => x.AddAsync(It.IsAny<PotluckItem>()), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_EmptyDescription_RejectsItem()
    {
        var repository = new Mock<BordspelApp.Application.Interfaces.IPotluckRepository>();
        var service = new PotluckService(repository.Object);

        var result = await service.AddItemAsync(new PotluckItemCreateDto
        {
            GameNightId = 10,
            PersonId = 20,
            Description = "   "
        });

        Assert.Null(result);
        repository.Verify(x => x.AddAsync(It.IsAny<PotluckItem>()), Times.Never);
    }

    [Fact]
    public async Task AddItemAsync_NonParticipant_RejectsItem()
    {
        var repository = new Mock<BordspelApp.Application.Interfaces.IPotluckRepository>();
        repository.Setup(x => x.GameNightExistsAsync(10)).ReturnsAsync(true);
        repository.Setup(x => x.IsParticipantAsync(10, 20)).ReturnsAsync(false);
        var service = new PotluckService(repository.Object);

        var result = await service.AddItemAsync(new PotluckItemCreateDto
        {
            GameNightId = 10,
            PersonId = 20,
            Description = "Chips"
        });

        Assert.Null(result);
        repository.Verify(x => x.AddAsync(It.IsAny<PotluckItem>()), Times.Never);
    }

    [Fact]
    public async Task AddItemAsync_PreservesDietaryCharacteristics()
    {
        var repository = new Mock<BordspelApp.Application.Interfaces.IPotluckRepository>();
        repository.Setup(x => x.GameNightExistsAsync(10)).ReturnsAsync(true);
        repository.Setup(x => x.IsParticipantAsync(10, 20)).ReturnsAsync(true);
        repository.Setup(x => x.AddAsync(It.IsAny<PotluckItem>()))
            .ReturnsAsync((PotluckItem item) => item);
        var service = new PotluckService(repository.Object);

        var result = await service.AddItemAsync(new PotluckItemCreateDto
        {
            GameNightId = 10,
            PersonId = 20,
            Description = "Groenteschotel",
            IsLactoseFree = true,
            IsNutFree = true,
            IsVegetarian = true,
            IsAlcoholFree = true
        });

        Assert.NotNull(result);
        Assert.True(result.IsLactoseFree);
        Assert.True(result.IsNutFree);
        Assert.True(result.IsVegetarian);
        Assert.True(result.IsAlcoholFree);
    }

    [Fact]
    public async Task GetItemsAsync_ReturnsItemsForGameNight()
    {
        var items = new List<PotluckItem>
        {
            new(10, 20, "Chips", false, true, true, true),
            new(10, 21, "Sap", true, true, true, true)
        };
        var repository = new Mock<BordspelApp.Application.Interfaces.IPotluckRepository>();
        repository.Setup(x => x.GetByGameNightAsync(10)).ReturnsAsync(items);
        var service = new PotluckService(repository.Object);

        var result = await service.GetItemsAsync(10);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, item => item.Description == "Chips");
        Assert.Contains(result, item => item.Description == "Sap" && item.IsLactoseFree);
    }

    [Fact]
    public async Task PotluckRepository_StoresAndReadsDietaryCharacteristics()
    {
        var options = new DbContextOptionsBuilder<BordspelApp.Infrastructure.Data.BordspelAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new BordspelApp.Infrastructure.Data.BordspelAppDbContext(options);
        var gameNight = Builders.CreateGameNight(id: 10);
        var person = Builders.CreatePerson(id: 20);
        context.GameNights.Add(gameNight);
        context.Persons.Add(person);
        context.GameNightPlayers.Add(new GameNightPlayer(10, 20));
        await context.SaveChangesAsync();

        var repository = new PotluckRepository(context);
        await repository.AddAsync(new PotluckItem(10, 20, "Vegan brownies", true, true, true, true));

        var items = await repository.GetByGameNightAsync(10);

        var item = Assert.Single(items);
        Assert.Equal("Vegan brownies", item.Description);
        Assert.True(item.IsLactoseFree);
        Assert.True(item.IsNutFree);
        Assert.True(item.IsVegetarian);
        Assert.True(item.IsAlcoholFree);
    }
}
