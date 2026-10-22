using BordspelApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BordspelApp.Infrastructure.Data;

public class BordspelAppIdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public BordspelAppIdentityDbContext(DbContextOptions<BordspelAppIdentityDbContext> options)
        : base(options)
    {
    }

    // Configureert Identity-tabellen zonder een cross-database foreign key naar Person.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Dit is een applicatielink naar de domeindatabase; EF maakt bewust geen foreign key.
        modelBuilder.Entity<ApplicationUser>()
            .Property(user => user.PersonId)
            .IsRequired(false);
    }
}
