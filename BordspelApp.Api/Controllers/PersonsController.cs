using BordspelApp.Application.Interfaces;
using BordspelApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BordspelApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonsController : ControllerBase
{
    private readonly IPersonRepository _personRepository;

    public PersonsController(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Person>>> GetAll()
    {
        var persons = await _personRepository.GetAllAsync();
        return Ok(persons);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Person>> GetById(int id)
    {
        var person = await _personRepository.GetByIdAsync(id);
        if (person == null)
        {
            return NotFound();
        }
        return Ok(person);
    }

    [HttpPost]
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Person person)
    {
        if (id != person.Id)
        {
            return BadRequest();
        }

        await _personRepository.UpdateAsync(person);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _personRepository.DeleteAsync(id);
        return NoContent();
    }
}
