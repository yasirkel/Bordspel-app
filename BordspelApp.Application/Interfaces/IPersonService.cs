using BordspelApp.Application.DTOs;

namespace BordspelApp.Application.Interfaces;

public interface IPersonService
{
    /// <summary>Haalt een profiel op via het ID.</summary>
    Task<PersonDto?> GetByIdAsync(int id);
    /// <summary>Haalt een profiel op via e-mailadres.</summary>
    Task<PersonDto?> GetByEmailAsync(string email);
    /// <summary>Werkt een profiel met adres en dieetvoorkeuren bij.</summary>
    Task<bool> UpdateProfileAsync(PersonDto dto);
    /// <summary>Geeft de actuele leeftijd van een persoon terug.</summary>
    Task<int> GetAgeAsync(int personId);
}
