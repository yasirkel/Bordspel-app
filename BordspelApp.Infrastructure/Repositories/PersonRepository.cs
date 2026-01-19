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

    public async Task<Person?> GetByIdAsync(int id)
    {
        return await _context.Persons.FindAsync(id);
    }

    public async Task<IEnumerable<Person>> GetAllAsync()
    {
        return await _context.Persons.ToListAsync();
    }

    public IQueryable<Person> GetAllQueryable()
    {
        return _context.Persons;
    }

    public async Task<Person?> GetByEmailAsync(string email)
    {
        return await _context.Persons.FirstOrDefaultAsync(p => p.Email == email);
    }

    public async Task<Person> AddAsync(Person person)
    {
        _context.Persons.Add(person);
        await _context.SaveChangesAsync();
        return person;
    }

    public async Task UpdateAsync(Person person)
    {
        _context.Persons.Update(person);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var person = await GetByIdAsync(id);
        if (person != null)
        {
            _context.Persons.Remove(person);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Person>> GetPersonsAtLeast18YearsOldAsync()
    {
        var cutoffDate = DateTime.Today.AddYears(-18);
        return await _context.Persons
            .Where(p => p.DateOfBirth <= cutoffDate)
            .ToListAsync();
    }
}
