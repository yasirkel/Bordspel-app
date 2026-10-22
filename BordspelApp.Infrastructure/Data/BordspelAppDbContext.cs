using BordspelApp.Domain.Entities;
using BordspelApp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BordspelApp.Infrastructure.Data;

public class BordspelAppDbContext : DbContext
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

    /// <summary>Configureert entiteiten, value objects en relaties in de domeindatabase.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configureer profielvelden, value objects en de organisatorrelatie.
        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(p => p.Email).IsUnique();

            // Adres wordt als value object in dezelfde tabel opgeslagen.
            entity.OwnsOne(p => p.Address, address =>
            {
                address.Property(a => a.Street).HasMaxLength(200);
                address.Property(a => a.HouseNumber).HasMaxLength(10);
                address.Property(a => a.City).HasMaxLength(100);
            });

            // Dieetvoorkeuren worden als value object opgeslagen.
            entity.OwnsOne(p => p.DietaryRequirements);

            // Eén persoon kan meerdere spelavonden organiseren.
            entity.HasMany(p => p.OrganizedGameNights)
                .WithOne(gn => gn.Organizer)
                .HasForeignKey(gn => gn.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configureer de bordspelvelden en maximale afbeeldings-URL-lengte.
        modelBuilder.Entity<BoardGame>(entity =>
        {
            entity.HasKey(bg => bg.Id);
            entity.Property(bg => bg.Name).IsRequired().HasMaxLength(200);
            entity.Property(bg => bg.Description).HasMaxLength(1000);
            entity.Property(bg => bg.PhotoUrl).HasMaxLength(500);
        });

        // Configureer de kerngegevens en het adres van een spelavond.
        modelBuilder.Entity<GameNight>(entity =>
        {
            entity.HasKey(gn => gn.Id);
            entity.Property(gn => gn.DateTime).IsRequired();
            entity.Property(gn => gn.MaxPlayers).IsRequired();

            // Het adres blijft onderdeel van de spelavond en krijgt geen eigen tabel.
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

        // Configureer de koppeltabel tussen spelavonden en deelnemers.
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

        // Configureer de koppeltabel tussen spelavonden en bordspellen.
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
