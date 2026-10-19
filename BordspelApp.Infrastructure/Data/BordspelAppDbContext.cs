using BordspelApp.Domain.Entities;
using BordspelApp.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BordspelApp.Infrastructure.Identity;

namespace BordspelApp.Infrastructure.Data;

public class BordspelAppDbContext : IdentityDbContext<ApplicationUser>
{
    public BordspelAppDbContext(DbContextOptions<BordspelAppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Person> Persons { get; set; }
    public DbSet<BoardGame> BoardGames { get; set; }
    public DbSet<GameNight> GameNights { get; set; }
    public DbSet<GameNightPlayer> GameNightPlayers { get; set; }
    public DbSet<GameNightBoardGame> GameNightBoardGames { get; set; }
    public DbSet<PotluckItem> PotluckItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApplicationUser ↔ Person (optional link)
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(u => u.Person)
                .WithMany()
                .HasForeignKey(u => u.PersonId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Person Configuration
        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(p => p.Email).IsUnique();

            // Owned type for Address
            entity.OwnsOne(p => p.Address, address =>
            {
                address.Property(a => a.Street).HasMaxLength(200);
                address.Property(a => a.HouseNumber).HasMaxLength(10);
                address.Property(a => a.City).HasMaxLength(100);
            });

            // Owned type for DietaryRequirements
            entity.OwnsOne(p => p.DietaryRequirements);

            // One-to-many: Person organizes many GameNights
            entity.HasMany(p => p.OrganizedGameNights)
                .WithOne(gn => gn.Organizer)
                .HasForeignKey(gn => gn.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // BoardGame Configuration
        modelBuilder.Entity<BoardGame>(entity =>
        {
            entity.HasKey(bg => bg.Id);
            entity.Property(bg => bg.Name).IsRequired().HasMaxLength(200);
            entity.Property(bg => bg.Description).HasMaxLength(1000);
            entity.Property(bg => bg.PhotoUrl).HasMaxLength(500);
        });

        // GameNight Configuration
        modelBuilder.Entity<GameNight>(entity =>
        {
            entity.HasKey(gn => gn.Id);
            entity.Property(gn => gn.DateTime).IsRequired();
            entity.Property(gn => gn.MaxPlayers).IsRequired();

            // Owned type for Address
            entity.OwnsOne(gn => gn.Address, address =>
            {
                address.Property(a => a.Street).HasMaxLength(200);
                address.Property(a => a.HouseNumber).HasMaxLength(10);
                address.Property(a => a.City).HasMaxLength(100);
            });
        });

        modelBuilder.Entity<PotluckItem>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Description).IsRequired().HasMaxLength(200);

            entity.HasOne(item => item.GameNight)
                .WithMany(gameNight => gameNight.PotluckItems)
                .HasForeignKey(item => item.GameNightId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.Person)
                .WithMany()
                .HasForeignKey(item => item.PersonId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // GameNightPlayer (Many-to-Many junction table)
        modelBuilder.Entity<GameNightPlayer>(entity =>
        {
            entity.HasKey(gnp => new { gnp.GameNightId, gnp.PersonId });

            entity.HasOne(gnp => gnp.GameNight)
                .WithMany(gn => gn.Players)
                .HasForeignKey(gnp => gnp.GameNightId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gnp => gnp.Person)
                .WithMany(p => p.GameNightParticipations)
                .HasForeignKey(gnp => gnp.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // GameNightBoardGame (Many-to-Many junction table)
        modelBuilder.Entity<GameNightBoardGame>(entity =>
        {
            entity.HasKey(gnbg => new { gnbg.GameNightId, gnbg.BoardGameId });

            entity.HasOne(gnbg => gnbg.GameNight)
                .WithMany(gn => gn.BoardGames)
                .HasForeignKey(gnbg => gnbg.GameNightId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gnbg => gnbg.BoardGame)
                .WithMany(bg => bg.GameNights)
                .HasForeignKey(gnbg => gnbg.BoardGameId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
