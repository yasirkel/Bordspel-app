using System.ComponentModel.DataAnnotations;

namespace BordspelApp.WebApp.Models;

public class ProfileEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Voornaam is verplicht")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Achternaam is verplicht")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is verplicht")]
    [EmailAddress(ErrorMessage = "Ongeldig emailadres")]
    public string Email { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }
    public int Age { get; set; }

    // Address
    public string? Street { get; set; }
    public string? HouseNumber { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }

    // Dietary preferences
    [Display(Name = "Lactosevrij")]
    public bool LactoseFree { get; set; }

    [Display(Name = "Notenvrij")]
    public bool NutFree { get; set; }

    [Display(Name = "Vegetarisch")]
    public bool Vegetarian { get; set; }

    [Display(Name = "Geen alcohol")]
    public bool NoAlcohol { get; set; }
}
