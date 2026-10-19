using BordspelApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BordspelApp.Infrastructure.Data;

// Retained only so the historical combined-database migrations remain
// identifiable without being applied to either new split database.
public class LegacyBordspelAppDbContext : IdentityDbContext<ApplicationUser>
{
    public LegacyBordspelAppDbContext(DbContextOptions<LegacyBordspelAppDbContext> options)
        : base(options)
    {
    }
}
