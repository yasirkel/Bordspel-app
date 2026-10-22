using BordspelApp.Application.Interfaces;
using BordspelApp.Domain.Entities;
using BordspelApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BordspelApp.Infrastructure.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly BordspelAppDbContext _context;

    public PersonRepository(BordspelAppDbContext context)
    {
        _context = context;
    }

    /// <summary>Haalt een domeinpersoon op via het ID.</summary>
    public async Task<Person?> GetByIdAsync(int id)
    {
        return await _context.Persons.FindAsync(id);
    }

    /// <summary>Haalt alle domeinpersonen op.</summary>
    public async Task<IEnumerable<Person>> GetAllAsync()
    {
        return await _context.Persons.ToListAsync();
    }

    /// <summary>Geeft een queryable terug voor profielqueries.</summary>
    public IQueryable<Person> GetAllQueryable()
    {
        return _context.Persons;
    }

    /// <summary>Zoekt een domeinpersoon op e-mailadres.</summary>
    public async Task<Person?> GetByEmailAsync(string email)
    {
        return await _context.Persons.FirstOrDefaultAsync(p => p.Email == email);
    }

    /// <summary>Slaat een nieuw domeinprofiel op.</summary>
    public async Task<Person> AddAsync(Person person)
    {
        _context.Persons.Add(person);
        await _context.SaveChangesAsync();
        return person;
    }

    /// <summary>Slaat wijzigingen aan een domeinprofiel op.</summary>
    public async Task UpdateAsync(Person person)
    {
        _context.Persons.Update(person);
        await _context.SaveChangesAsync();
    }

    /// <summary>Verwijdert een profiel wanneer het ID bestaat.</summary>
    public async Task DeleteAsync(int id)
    {
        var person = await GetByIdAsync(id);
        if (person != null)
        {
            _context.Persons.Remove(person);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>Haalt alle personen op die volgens hun geboortedatum volwassen zijn.</summary>
    public async Task<IEnumerable<Person>> GetPersonsAtLeast18YearsOldAsync()
    {
        var cutoffDate = DateTime.Today.AddYears(-18);
        return await _context.Persons
            .Where(p => p.DateOfBirth <= cutoffDate)
            .ToListAsync();
    }
}
