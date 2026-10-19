namespace BordspelApp.Domain.Entities;

public class PotluckItem
{
    public int Id { get; set; }
    public int GameNightId { get; set; }
    public GameNight GameNight { get; set; } = null!;

    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public string Description { get; private set; } = string.Empty;
    public bool IsLactoseFree { get; private set; }
    public bool IsNutFree { get; private set; }
    public bool IsVegetarian { get; private set; }
    public bool IsAlcoholFree { get; private set; }

    private PotluckItem()
    {
    }

    public PotluckItem(
        int gameNightId,
        int personId,
        string description,
        bool isLactoseFree,
        bool isNutFree,
        bool isVegetarian,
        bool isAlcoholFree)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Een potluckitem moet een omschrijving hebben.", nameof(description));
        }

        GameNightId = gameNightId;
        PersonId = personId;
        Description = description.Trim();
        IsLactoseFree = isLactoseFree;
        IsNutFree = isNutFree;
        IsVegetarian = isVegetarian;
        IsAlcoholFree = isAlcoholFree;
    }
}
