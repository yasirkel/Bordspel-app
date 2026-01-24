using System;

namespace BordspelApp.Api.DTOs;

public class PersonSummaryDto
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}
