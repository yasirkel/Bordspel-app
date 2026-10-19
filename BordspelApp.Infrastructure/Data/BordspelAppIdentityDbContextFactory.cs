using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BordspelApp.Infrastructure.Data;

public class BordspelAppIdentityDbContextFactory : IDesignTimeDbContextFactory<BordspelAppIdentityDbContext>
{
    public BordspelAppIdentityDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("BORDSPELAPP_IDENTITY_CONNECTION")
            ?? "Server=(localdb)\\mssqllocaldb;Database=BordspelAppIdentityDb;Trusted_Connection=true;MultipleActiveResultSets=true";

        var options = new DbContextOptionsBuilder<BordspelAppIdentityDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new BordspelAppIdentityDbContext(options);
    }
}
