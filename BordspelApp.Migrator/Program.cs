using BordspelApp.Infrastructure.Data;
using BordspelApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

static string Required(string name) =>
    Environment.GetEnvironmentVariable(name)
    ?? throw new InvalidOperationException($"Required environment variable '{name}' is missing.");

var domainConnection = Required("DomainConnection");
var identityConnection = Required("IdentityConnection");

var domainOptions = new DbContextOptionsBuilder<BordspelAppDbContext>()
    .UseSqlServer(domainConnection)
    .Options;
var identityOptions = new DbContextOptionsBuilder<BordspelAppIdentityDbContext>()
    .UseSqlServer(identityConnection)
    .Options;

await using (var domain = new BordspelAppDbContext(domainOptions))
{
    Console.WriteLine("Applying domain migrations...");
    await domain.Database.MigrateAsync();
}

await using (var identity = new BordspelAppIdentityDbContext(identityOptions))
{
    Console.WriteLine("Applying Identity migrations...");
    await identity.Database.MigrateAsync();
}

if (string.Equals(Environment.GetEnvironmentVariable("ASSESSMENT_SEED"), "true", StringComparison.OrdinalIgnoreCase))
{
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddIdentityCore<ApplicationUser>()
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<BordspelAppIdentityDbContext>();
    services.AddSingleton(new BordspelAppIdentityDbContext(identityOptions));
    services.AddSingleton(new BordspelAppDbContext(domainOptions));

    await using var provider = services.BuildServiceProvider();
    await AssessmentDataSeeder.SeedAsync(
        provider.GetRequiredService<BordspelAppDbContext>(),
        provider.GetRequiredService<UserManager<ApplicationUser>>(),
        provider.GetRequiredService<RoleManager<IdentityRole>>());
    Console.WriteLine("Assessment demonstration seed completed.");
}

Console.WriteLine("Database migrations completed.");
