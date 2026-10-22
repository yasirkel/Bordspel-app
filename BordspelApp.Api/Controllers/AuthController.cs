using BordspelApp.Api.DTOs;
using BordspelApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BordspelApp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    /// <summary>Controleert de Identity-inloggegevens en zet een authenticatiecookie.</summary>
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _signInManager.UserManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);
        if (result.IsLockedOut)
        {
            return StatusCode(StatusCodes.Status423Locked, new { message = "Account is temporarily locked." });
        }

        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        await _signInManager.SignInAsync(user, isPersistent: false);

        return Ok(new { message = "Authenticated. The Identity cookie is active for this client." });
    }

    [Authorize]
    [HttpPost("logout")]
    /// <summary>Verwijdert de actieve Identity-authenticatiecookie.</summary>
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }
}
