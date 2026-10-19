namespace BordspelApp.Application.DTOs;

public class PotluckItemDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsLactoseFree { get; set; }
    public bool IsNutFree { get; set; }
    public bool IsVegetarian { get; set; }
    public bool IsAlcoholFree { get; set; }
}

public class PotluckItemCreateDto
{
    public int GameNightId { get; set; }
    public int PersonId { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsLactoseFree { get; set; }
    public bool IsNutFree { get; set; }
    public bool IsVegetarian { get; set; }
    public bool IsAlcoholFree { get; set; }
}
