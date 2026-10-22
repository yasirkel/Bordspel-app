using BordspelApp.Application.DTOs;
using BordspelApp.Application.Interfaces;
using BordspelApp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BordspelApp.Infrastructure.Services;

public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;

    public PersonService(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    // Haalt een profiel met adres en dieetvoorkeuren op basis van het ID op.
    public async Task<PersonDto?> GetByIdAsync(int id)
    {
        var person = await _personRepository.GetAllQueryable()
            .AsNoTracking()
            .Include(p => p.Address)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (person == null) return null;

        return new PersonDto
        {
            Id = person.Id,
            FirstName = person.FirstName,
            LastName = person.LastName,
            DateOfBirth = person.DateOfBirth,
            Email = person.Email,
            Street = person.Address?.Street,
            HouseNumber = person.Address?.HouseNumber,
            City = person.Address?.City,
            PostalCode = person.Address?.PostalCode,
            LactoseFree = person.DietaryRequirements.IsLactoseFree,
            NutFree = person.DietaryRequirements.IsNutFree,
            Vegetarian = person.DietaryRequirements.IsVegetarian,
            NoAlcohol = person.DietaryRequirements.IsAlcoholFree
        };
    }

    // Haalt een profiel op via het e-mailadres van de gekoppelde gebruiker.
    public async Task<PersonDto?> GetByEmailAsync(string email)
    {
        var person = await _personRepository.GetAllQueryable()
            .AsNoTracking()
            .Include(p => p.Address)
            .FirstOrDefaultAsync(p => p.Email == email);

        if (person == null) return null;

        return new PersonDto
        {
            Id = person.Id,
            FirstName = person.FirstName,
            LastName = person.LastName,
            DateOfBirth = person.DateOfBirth,
            Email = person.Email,
            Street = person.Address?.Street,
            HouseNumber = person.Address?.HouseNumber,
            City = person.Address?.City,
            PostalCode = person.Address?.PostalCode,
            LactoseFree = person.DietaryRequirements.IsLactoseFree,
            NutFree = person.DietaryRequirements.IsNutFree,
            Vegetarian = person.DietaryRequirements.IsVegetarian,
            NoAlcohol = person.DietaryRequirements.IsAlcoholFree
        };
    }

    // Werk profielgegevens, dieetvoorkeuren en adres bij.
    public async Task<bool> UpdateProfileAsync(PersonDto dto)
    {
        var person = await _personRepository.GetAllQueryable()
            .Include(p => p.Address)
            .FirstOrDefaultAsync(p => p.Id == dto.Id);

        if (person == null) return false;

        person.FirstName = dto.FirstName;
        person.LastName = dto.LastName;
        person.Email = dto.Email ?? person.Email;
        
        // Werk de dieetvoorkeuren van het profiel bij.
        person.DietaryRequirements.IsLactoseFree = dto.LactoseFree;
        person.DietaryRequirements.IsNutFree = dto.NutFree;
        person.DietaryRequirements.IsVegetarian = dto.Vegetarian;
        person.DietaryRequirements.IsAlcoholFree = dto.NoAlcohol;

        // Werk het adres bij of maak een adres aan als dat nog ontbreekt.
        if (!string.IsNullOrWhiteSpace(dto.Street) && !string.IsNullOrWhiteSpace(dto.City))
        {
            if (person.Address == null)
            {
                person.Address = new Address(
                    dto.Street, 
                    dto.HouseNumber ?? "", 
                    dto.City, 
                    dto.PostalCode ?? ""
                );
            }
            else
            {
                person.Address.UpdateAddress(
                    dto.Street, 
                    dto.HouseNumber ?? "", 
                    dto.City, 
                    dto.PostalCode ?? ""
                );
            }
        }

        await _personRepository.UpdateAsync(person);
        return true;
    }

    // Bereken de actuele leeftijd van een persoon.
    public async Task<int> GetAgeAsync(int personId)
    {
        var person = await _personRepository.GetAllQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == personId);

        if (person == null) return 0;

        var today = DateTime.Today;
        var age = today.Year - person.DateOfBirth.Year;
        if (person.DateOfBirth.Date > today.AddYears(-age)) age--;
        
        return age;
    }
}
