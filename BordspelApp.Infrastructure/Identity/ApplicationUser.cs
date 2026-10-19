using Microsoft.AspNetCore.Identity;

namespace BordspelApp.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public int? PersonId { get; set; }
}
