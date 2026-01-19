namespace BordspelApp.Domain.ValueObjects;

public class DietaryRequirements
{
    public bool IsLactoseFree { get; set; }
    public bool IsNutFree { get; set; }
    public bool IsVegetarian { get; set; }
    public bool IsAlcoholFree { get; set; }

    public DietaryRequirements()
    {
    }

    public DietaryRequirements(bool isLactoseFree, bool isNutFree, bool isVegetarian, bool isAlcoholFree)
    {
        IsLactoseFree = isLactoseFree;
        IsNutFree = isNutFree;
        IsVegetarian = isVegetarian;
        IsAlcoholFree = isAlcoholFree;
    }
}
