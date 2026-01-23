using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using BordspelApp.Infrastructure.Identity;
using BordspelApp.Application.Interfaces;

namespace BordspelApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPersonRepository _personRepository;

    public AdminController(UserManager<ApplicationUser> userManager, IPersonRepository personRepository)
    {
        _userManager = userManager;
        _personRepository = personRepository;
    }

    public class LinkUserPersonRequest
    {
        public string Email { get; set; } = string.Empty;
        public int PersonId { get; set; }
    }

    [HttpPost("link-user-person")]
    public async Task<IActionResult> LinkUserPerson([FromBody] LinkUserPersonRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email)) return BadRequest("Email is required");

        var person = await _personRepository.GetByIdAsync(req.PersonId);
        if (person == null) return NotFound($"Person with id {req.PersonId} not found");

        var user = await _userManager.FindByEmailAsync(req.Email);
        if (user == null) return NotFound($"User with email {req.Email} not found");

        user.PersonId = req.PersonId;
        var res = await _userManager.UpdateAsync(user);
        if (!res.Succeeded)
        {
            return StatusCode(500, new { errors = res.Errors.Select(e => e.Description) });
        }

        return Ok(new { user.Id, user.Email, user.PersonId });
    }
}
