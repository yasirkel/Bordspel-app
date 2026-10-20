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

    public async Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal)
    {
        return await _userManager.GetUserAsync(principal);
    }

    public async Task<int?> GetPersonIdAsync(ClaimsPrincipal principal)
    {
        var user = await GetUserAsync(principal);
        return user?.PersonId;
    }
}
