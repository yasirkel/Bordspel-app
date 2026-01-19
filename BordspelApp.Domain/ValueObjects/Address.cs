namespace BordspelApp.Domain.ValueObjects;

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public Address()
    {
    }

    public Address(string street, string houseNumber, string city, string postalCode = "")
    {
        Street = street;
        HouseNumber = houseNumber;
        City = city;
        PostalCode = postalCode;
    }

    public void UpdateAddress(string street, string houseNumber, string city, string postalCode)
    {
        Street = street;
        HouseNumber = houseNumber;
        City = city;
        PostalCode = postalCode;
    }

    public override string ToString()
    {
        return $"{Street} {HouseNumber}, {PostalCode} {City}";
    }
}
