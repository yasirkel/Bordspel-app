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

    /// <summary>Haalt de Identity-gebruiker op die bij de huidige claims hoort.</summary>
    public async Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal)
    {
        return await _userManager.GetUserAsync(principal);
    }

    /// <summary>Geeft het gekoppelde Person-ID van de ingelogde gebruiker terug.</summary>
    public async Task<int?> GetPersonIdAsync(ClaimsPrincipal principal)
    {
        var user = await GetUserAsync(principal);
        return user?.PersonId;
    }
}
