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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // This is an application-level link to the domain database.
        // It intentionally has no EF relationship or foreign key.
        modelBuilder.Entity<ApplicationUser>()
            .Property(user => user.PersonId)
            .IsRequired(false);
    }
}
