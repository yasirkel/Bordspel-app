using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using BordspelApp.Infrastructure.Identity;
using BordspelApp.Application.Interfaces;

namespace BordspelApp.WebApp.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPersonRepository _personRepository;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<LoginModel> logger, IPersonRepository personRepository)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _personRepository = personRepository;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in.");

            // SAFETY NET: ensure the Identity user is linked to a domain Person
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user != null && user.PersonId == null)
            {
                try
                {
                    var person = await _personRepository.GetByEmailAsync(user.Email);
                    if (person != null)
                    {
                        user.PersonId = person.Id;
                        var ures = await _userManager.UpdateAsync(user);
                        if (!ures.Succeeded)
                        {
                            _logger.LogWarning("Failed to set PersonId on user {Email}: {Errors}", user.Email, string.Join(';', ures.Errors.Select(e => e.Description)));
                        }
                        else
                        {
                            _logger.LogInformation("Linked user {Email} to Person {PersonId} during login.", user.Email, person.Id);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning(ex, "Safety-net linking failed for user {Email}", user.Email);
                }
            }

            return LocalRedirect(returnUrl);
        }
        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out.");
            return RedirectToPage("./Lockout");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }
}
