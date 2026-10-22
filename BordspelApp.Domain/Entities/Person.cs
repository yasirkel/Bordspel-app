using BordspelApp.Domain.Enums;
using BordspelApp.Domain.ValueObjects;

namespace BordspelApp.Domain.Entities;

public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }

    // Address
    public Address Address { get; set; } = new();

    // Dietary Requirements
    public DietaryRequirements DietaryRequirements { get; set; } = new();

    // Navigation properties
    public ICollection<GameNight> OrganizedGameNights { get; set; } = new List<GameNight>();
    public ICollection<GameNightPlayer> GameNightParticipations { get; set; } = new List<GameNightPlayer>();

    // Berekent de actuele leeftijd op basis van de geboortedatum.
    public int Age
    {
        get
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }
            return age;
        }
    }

    // Geeft de volledige naam voor weergave in de applicatie.
    public string FullName => $"{FirstName} {LastName}";

    // Controleert of de geboortedatum niet in de toekomst ligt.
    public bool IsValidDateOfBirth()
    {
        return DateOfBirth <= DateTime.Today;
    }

    // Controleert of de persoon minimaal zestien jaar oud is.
    public bool IsAtLeast16YearsOld()
    {
        return Age >= 16;
    }

    // Controleert of de persoon minimaal achttien jaar oud is.
    public bool IsAtLeast18YearsOld()
    {
        return Age >= 18;
    }

    // Controleert of de persoon als organisator mag optreden.
    public bool CanOrganizeGameNight()
    {
        return IsAtLeast18YearsOld();
    }
}
