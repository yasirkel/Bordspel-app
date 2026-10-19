using BordspelApp.Infrastructure.Data;
using BordspelApp.Infrastructure.Identity;
using BordspelApp.Domain.Entities;
using System;
using System.Linq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace BordspelApp.Tests.Infrastructure;

public class SeparatedDatabaseContextTests
{
    [Fact]
    public void DomainContext_ContainsOnlyDomainEntities()
    {
        var options = new DbContextOptionsBuilder<BordspelAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new BordspelAppDbContext(options);
        var entityTypes = context.Model.GetEntityTypes().Select(entity => entity.ClrType);

        Assert.Contains(typeof(Person), entityTypes);
        Assert.Contains(typeof(GameNight), entityTypes);
        Assert.Contains(typeof(PotluckItem), entityTypes);
        Assert.DoesNotContain(typeof(ApplicationUser), entityTypes);
    }

    [Fact]
    public void IdentityContext_ContainsIdentityEntitiesWithoutPersonForeignKey()
    {
        var options = new DbContextOptionsBuilder<BordspelAppIdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new BordspelAppIdentityDbContext(options);
        var userEntity = context.Model.FindEntityType(typeof(ApplicationUser));

        Assert.NotNull(userEntity);
        Assert.Contains(context.Model.GetEntityTypes(), entity => entity.ClrType == typeof(ApplicationUser));
        Assert.DoesNotContain(context.Model.GetEntityTypes(), entity => entity.ClrType == typeof(Person));
        Assert.Empty(userEntity.GetForeignKeys());
        Assert.NotNull(userEntity.FindProperty(nameof(ApplicationUser.PersonId)));
    }

    [Fact]
    public void ApplicationUser_RetainsApplicationLevelPersonLink()
    {
        var user = new ApplicationUser { PersonId = 42 };

        Assert.Equal(42, user.PersonId);
    }
}
