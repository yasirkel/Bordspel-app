using BordspelApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BordspelApp.Api.Services;

public sealed class CurrentUserAccessor
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CurrentUserAccessor(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // Haalt de Identity-gebruiker op die bij de huidige claims hoort.
    public async Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal)
    {
        return await _userManager.GetUserAsync(principal);
    }

    // Geeft het gekoppelde Person-ID van de ingelogde gebruiker terug.
    public async Task<int?> GetPersonIdAsync(ClaimsPrincipal principal)
    {
        var user = await GetUserAsync(principal);
        return user?.PersonId;
    }
}
