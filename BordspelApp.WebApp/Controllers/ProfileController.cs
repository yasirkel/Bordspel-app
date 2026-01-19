using BordspelApp.Application.DTOs;
using BordspelApp.Application.Interfaces;
using BordspelApp.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BordspelApp.WebApp.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IPersonService _personService;

    public ProfileController(IPersonService personService)
    {
        _personService = personService;
    }

    // GET: /Profile/Edit
    public async Task<IActionResult> Edit()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            TempData["Error"] = "Gebruiker niet gevonden.";
            return RedirectToAction("Index", "Home");
        }

        var person = await _personService.GetByEmailAsync(email);
        if (person == null)
        {
            TempData["Error"] = "Profiel niet gevonden.";
            return RedirectToAction("Index", "Home");
        }

        var viewModel = new ProfileEditViewModel
        {
            Id = person.Id,
            FirstName = person.FirstName,
            LastName = person.LastName,
            Email = person.Email ?? "",
            DateOfBirth = person.DateOfBirth,
            Age = person.Age,
            Street = person.Street,
            HouseNumber = person.HouseNumber,
            City = person.City,
            PostalCode = person.PostalCode,
            LactoseFree = person.LactoseFree,
            NutFree = person.NutFree,
            Vegetarian = person.Vegetarian,
            NoAlcohol = person.NoAlcohol
        };

        return View(viewModel);
    }

    // POST: /Profile/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var dto = new PersonDto
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            DateOfBirth = model.DateOfBirth,
            Street = model.Street,
            HouseNumber = model.HouseNumber,
            City = model.City,
            PostalCode = model.PostalCode,
            LactoseFree = model.LactoseFree,
            NutFree = model.NutFree,
            Vegetarian = model.Vegetarian,
            NoAlcohol = model.NoAlcohol
        };

        var success = await _personService.UpdateProfileAsync(dto);
        if (success)
        {
            TempData["Success"] = "Profiel succesvol bijgewerkt!";
            return RedirectToAction(nameof(Edit));
        }

        TempData["Error"] = "Er ging iets mis bij het bijwerken van je profiel.";
        return View(model);
    }
}
