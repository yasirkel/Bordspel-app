using BordspelApp.Application.Interfaces;
using BordspelApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BordspelApp.Api.Services;

namespace BordspelApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonsController : ControllerBase
{
    private readonly IPersonRepository _personRepository;
    private readonly CurrentUserAccessor _currentUser;

    public PersonsController(IPersonRepository personRepository, CurrentUserAccessor currentUser)
    {
        _personRepository = personRepository;
        _currentUser = currentUser;
    }

    [HttpGet]
    /// <summary>Geeft alle domeinprofielen terug.</summary>
    public async Task<ActionResult<IEnumerable<Person>>> GetAll()
    {
        var persons = await _personRepository.GetAllAsync();
        return Ok(persons);
    }

    [HttpGet("{id}")]
    /// <summary>Geeft één profiel terug op basis van het Person-ID.</summary>
    public async Task<ActionResult<Person>> GetById(int id)
    {
        var person = await _personRepository.GetByIdAsync(id);
        if (person == null)
        {
            return NotFound();
        }
        return Ok(person);
    }

    [Authorize]
    [HttpPost]
    /// <summary>Maakt een profiel aan na controle van de minimale leeftijd.</summary>
    public async Task<ActionResult<Person>> Create(Person person)
    {
        if (!person.IsValidDateOfBirth())
        {
            return BadRequest("Date of birth cannot be in the future.");
        }

        if (!person.IsAtLeast16YearsOld())
        {
            return BadRequest("Person must be at least 16 years old.");
        }

        var created = await _personRepository.AddAsync(person);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize]
    [HttpPut("{id}")]
    /// <summary>Wijzigt het eigen profiel of een profiel als beheerder.</summary>
    public async Task<IActionResult> Update(int id, Person person)
    {
        if (id != person.Id)
        {
            return BadRequest();
        }

        var currentPersonId = await _currentUser.GetPersonIdAsync(User);
        if (currentPersonId != id && !User.IsInRole("Admin")) return Forbid();
        if (!person.IsValidDateOfBirth() || !person.IsAtLeast16YearsOld()) return BadRequest("Person does not meet age requirements.");

        await _personRepository.UpdateAsync(person);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    /// <summary>Verwijdert het eigen profiel of een profiel als beheerder.</summary>
    public async Task<IActionResult> Delete(int id)
    {
        var currentPersonId = await _currentUser.GetPersonIdAsync(User);
        if (currentPersonId != id && !User.IsInRole("Admin")) return Forbid();
        await _personRepository.DeleteAsync(id);
        return NoContent();
    }
}
