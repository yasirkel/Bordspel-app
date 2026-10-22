using BordspelApp.Application.DTOs;

namespace BordspelApp.Application.Interfaces;

public interface IPersonService
{
    // Haalt een profiel op via het ID.
    Task<PersonDto?> GetByIdAsync(int id);
    // Haalt een profiel op via e-mailadres.
    Task<PersonDto?> GetByEmailAsync(string email);
    // Werkt een profiel met adres en dieetvoorkeuren bij.
    Task<bool> UpdateProfileAsync(PersonDto dto);
    // Geeft de actuele leeftijd van een persoon terug.
    Task<int> GetAgeAsync(int personId);
}
