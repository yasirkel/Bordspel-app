using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BordspelApp.Infrastructure.Data;

public class BordspelAppDbContextFactory : IDesignTimeDbContextFactory<BordspelAppDbContext>
{
    public BordspelAppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("BORDSPELAPP_DOMAIN_CONNECTION")
            ?? "Server=(localdb)\\mssqllocaldb;Database=BordspelAppDomainDb;Trusted_Connection=true;MultipleActiveResultSets=true";

        var options = new DbContextOptionsBuilder<BordspelAppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new BordspelAppDbContext(options);
    }
}
