using BordspelApp.Domain.Entities;

namespace BordspelApp.Application.Interfaces;

public interface IPersonRepository
{
    Task<Person?> GetByIdAsync(int id);
    Task<IEnumerable<Person>> GetAllAsync();
    IQueryable<Person> GetAllQueryable();
    Task<Person?> GetByEmailAsync(string email);
    Task<Person> AddAsync(Person person);
    Task UpdateAsync(Person person);
    Task DeleteAsync(int id);
    Task<IEnumerable<Person>> GetPersonsAtLeast18YearsOldAsync();
}
