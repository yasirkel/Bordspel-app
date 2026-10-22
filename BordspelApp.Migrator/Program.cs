using BordspelApp.Infrastructure.Data;
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

Console.WriteLine("Database migrations completed.");
