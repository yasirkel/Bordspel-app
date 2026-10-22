using BordspelApp.Application.Interfaces;
using BordspelApp.Domain.Entities;
using BordspelApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BordspelApp.WebApp.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPersonRepository _personRepository;

    public AccountController(UserManager<ApplicationUser> userManager, IPersonRepository personRepository)
    {
        _userManager = userManager;
        _personRepository = personRepository;
    }

    [HttpGet]
    // Toont het formulier waarmee een ingelogde gebruiker een profiel maakt.
    public async Task<IActionResult> CreateProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.PersonId != null) return RedirectToAction("Index", "Home");
        return View(new CreateProfileViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Valideert en bewaart het nieuwe profiel van de ingelogde gebruiker.
    public async Task<IActionResult> CreateProfile(CreateProfileViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var person = new Person
        {
            FirstName = vm.FirstName,
            LastName = vm.LastName,
            Email = vm.Email,
            DateOfBirth = vm.DateOfBirth,
            Gender = vm.Gender,
            Address = new BordspelApp.Domain.ValueObjects.Address(vm.Street, vm.HouseNumber, vm.City)
        };

        person.DietaryRequirements = new BordspelApp.Domain.ValueObjects.DietaryRequirements
        {
            IsAlcoholFree = vm.IsAlcoholFree,
            IsLactoseFree = vm.IsLactoseFree,
            IsNutFree = vm.IsNutFree,
            IsVegetarian = vm.IsVegetarian
        };

        person = await _personRepository.AddAsync(person);
        user.PersonId = person.Id;
        await _userManager.UpdateAsync(user);
        return RedirectToAction("Index", "Home");
    }
}

public class CreateProfileViewModel
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public DateTime DateOfBirth { get; set; }
    [Required]
    public BordspelApp.Domain.Enums.Gender Gender { get; set; }

    public string Street { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public bool IsLactoseFree { get; set; }
    public bool IsNutFree { get; set; }
    public bool IsVegetarian { get; set; }
    public bool IsAlcoholFree { get; set; }
}
