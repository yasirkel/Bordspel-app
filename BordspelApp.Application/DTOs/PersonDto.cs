namespace BordspelApp.Application.DTOs;

public class PersonDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Email { get; set; }
    
    // Address
    public string? Street { get; set; }
    public string? HouseNumber { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    
    // Dietary preferences
    public bool LactoseFree { get; set; }
    public bool NutFree { get; set; }
    public bool Vegetarian { get; set; }
    public bool NoAlcohol { get; set; }
    
    public int Age => CalculateAge();
    
    private int CalculateAge()
    {
        var today = DateTime.Today;
        var age = today.Year - DateOfBirth.Year;
        if (DateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}
