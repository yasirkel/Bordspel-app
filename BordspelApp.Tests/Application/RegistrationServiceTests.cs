using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BordspelApp.Application.Interfaces;
using BordspelApp.Application.Models;
using BordspelApp.Domain.Entities;
using BordspelApp.Infrastructure.Services;
using BordspelApp.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BordspelApp.Tests.Application;

public class RegistrationServiceTests
{
    [Fact]
    public async Task RegisterAsync_ExistingRegistrationSameDay_ReturnsAlreadyRegisteredSameDay()
    {
        // Arrange
        var person = Builders.CreatePerson(id: 20, age: 25);
        var gameNight = Builders.CreateGameNight(id: 100, dateTime: DateTime.Now.AddDays(1));

        var mockPersonRepo = new Mock<IPersonRepository>();
        var mockGameNightRepo = new Mock<IGameNightRepository>();

        var options = new DbContextOptionsBuilder<BordspelApp.Infrastructure.Data.BordspelAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using (var context = new BordspelApp.Infrastructure.Data.BordspelAppDbContext(options))
        {
            context.Persons.Add(person);
            context.GameNights.Add(gameNight);
            // Participant has a different GameNight on same day
            var otherGn = Builders.CreateGameNight(id: 200, dateTime: gameNight.DateTime);
            context.GameNights.Add(otherGn);
            context.SaveChanges();

            mockPersonRepo.Setup(r => r.GetAllQueryable()).Returns(context.Persons);
            mockGameNightRepo.Setup(r => r.GetAllQueryable()).Returns(context.GameNights);
            mockGameNightRepo.Setup(r => r.GetGameNightsByParticipantAsync(person.Id)).ReturnsAsync(new List<GameNight> { otherGn });

            var service = new RegistrationService(mockGameNightRepo.Object, mockPersonRepo.Object);

            // Act
            var result = await service.RegisterAsync(person.Id, gameNight.Id);

            // Assert
            Assert.Equal(RegistrationResult.AlreadyRegisteredSameDay, result.Result);
        }
    }

    [Fact]
    public async Task RegisterAsync_DietaryMismatch_ReturnsDietaryMismatchWithWarnings()
    {
        var person = Builders.CreatePerson(id: 21, age: 30);
        person.DietaryRequirements.IsLactoseFree = true;

        var gameNight = Builders.CreateGameNight(id: 101, dateTime: DateTime.Now.AddDays(2));
        gameNight.HasLactoseFreeOption = false;

        var mockPersonRepo2 = new Mock<IPersonRepository>();
        var mockGameNightRepo2 = new Mock<IGameNightRepository>();

        var options2 = new DbContextOptionsBuilder<BordspelApp.Infrastructure.Data.BordspelAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using (var context2 = new BordspelApp.Infrastructure.Data.BordspelAppDbContext(options2))
        {
            context2.Persons.Add(person);
            context2.GameNights.Add(gameNight);
            context2.SaveChanges();

            mockPersonRepo2.Setup(r => r.GetAllQueryable()).Returns(context2.Persons);
            mockGameNightRepo2.Setup(r => r.GetAllQueryable()).Returns(context2.GameNights);
            mockGameNightRepo2.Setup(r => r.GetGameNightsByParticipantAsync(person.Id)).ReturnsAsync(new List<GameNight>());
            mockGameNightRepo2.Setup(r => r.UpdateAsync(It.IsAny<GameNight>())).Returns(Task.CompletedTask);

            var service2 = new RegistrationService(mockGameNightRepo2.Object, mockPersonRepo2.Object);

            var result = await service2.RegisterAsync(person.Id, gameNight.Id);

            Assert.Equal(RegistrationResult.DietaryMismatch, result.Result);
            Assert.NotEmpty(result.DietaryWarnings);
        }
    }
}
