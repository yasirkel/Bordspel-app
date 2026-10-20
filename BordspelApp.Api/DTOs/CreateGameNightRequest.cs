using System.ComponentModel.DataAnnotations;
using BordspelApp.Domain.ValueObjects;

namespace BordspelApp.Api.DTOs;

public class CreateGameNightRequest
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DateTime { get; set; }
    [Range(1, 100)]
    public int MaxPlayers { get; set; }
    [Required]
    public Address Address { get; set; } = new();
    public bool IsPotluck { get; set; }
    public List<int> BoardGameIds { get; set; } = new();
}
