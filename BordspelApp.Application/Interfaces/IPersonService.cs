using BordspelApp.Application.DTOs;

namespace BordspelApp.Application.Interfaces;

public interface IPersonService
{
    Task<PersonDto?> GetByIdAsync(int id);
    Task<PersonDto?> GetByEmailAsync(string email);
    Task<bool> UpdateProfileAsync(PersonDto dto);
    Task<int> GetAgeAsync(int personId);
}
