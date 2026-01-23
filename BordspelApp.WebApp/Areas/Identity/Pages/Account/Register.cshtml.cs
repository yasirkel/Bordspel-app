// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using BordspelApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using BordspelApp.Application.Interfaces;
using BordspelApp.Domain.Entities;
using BordspelApp.Domain.ValueObjects;
using BordspelApp.Domain.Enums;

namespace BordspelApp.WebApp.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        private readonly IPersonRepository _personRepository;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IPersonRepository personRepository)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _personRepository = personRepository;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            public string FirstName { get; set; }

            [Required]
            public string LastName { get; set; }
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            public string PhoneNumber { get; set; }

            [Required]
            public string Address { get; set; }

            [DataType(DataType.Date)]
            public DateTime? DateOfBirth { get; set; }

            // Dietary preferences / allergies
            [Display(Name = "Lactose free")]
            public bool IsLactoseFree { get; set; }

            [Display(Name = "Nut free")]
            public bool IsNutFree { get; set; }

            [Display(Name = "Vegetarian")]
            public bool IsVegetarian { get; set; }

            [Display(Name = "No alcohol")]
            public bool IsAlcoholFree { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            // Enforce minimum age: user must be 16 or older
            if (!Input.DateOfBirth.HasValue)
            {
                ModelState.AddModelError("Input.DateOfBirth", "Date of birth is required.");
            }
            else
            {
                var dob = Input.DateOfBirth.Value.Date;
                var today = DateTime.UtcNow.Date;
                var age = today.Year - dob.Year;
                if (dob > today.AddYears(-age)) age--;
                if (age < 16)
                {
                    ModelState.AddModelError("Input.DateOfBirth", "You must be at least 16 years old to register.");
                }
            }
            if (!ModelState.IsValid)
            {
                // Log validation errors for debugging
                var errors = ModelState
                    .Where(kvp => kvp.Value.Errors.Count > 0)
                    .Select(kvp => new { Key = kvp.Key, Errors = kvp.Value.Errors.Select(e => e.ErrorMessage) });
                foreach (var e in errors)
                {
                    _logger.LogWarning("Register validation error for {Key}: {Errors}", e.Key, string.Join("; ", e.Errors));
                }

                return Page();
            }

            // proceed when model is valid
            if (ModelState.IsValid)
            {
                // Create Identity user using user store (avoid assuming extra properties exist)
                var user = CreateUser();
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    // set phone number if available
                    if (!string.IsNullOrWhiteSpace(Input.PhoneNumber))
                    {
                        await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                    }

                    // Create a minimal domain Person record and store dietary preferences (use provided DOB)
                    try
                    {
                        var person = new Person
                        {
                            FirstName = Input.FirstName,
                            LastName = Input.LastName,
                            Email = Input.Email,
                            DateOfBirth = Input.DateOfBirth ?? DateTime.UtcNow.AddYears(-18),
                            Gender = Gender.Other,
                            Address = new BordspelApp.Domain.ValueObjects.Address(Input.Address, string.Empty, string.Empty),
                            DietaryRequirements = new DietaryRequirements(Input.IsLactoseFree, Input.IsNutFree, Input.IsVegetarian, Input.IsAlcoholFree)
                        };

                        var added = await _personRepository.AddAsync(person);
                        // persist link between Identity user and domain Person
                        try
                        {
                            user.PersonId = added.Id;
                            var updateRes = await _userManager.UpdateAsync(user);
                            if (!updateRes.Succeeded)
                            {
                                _logger.LogWarning("Failed to update ApplicationUser with PersonId for {Email}: {Errors}", Input.Email, string.Join(';', updateRes.Errors.Select(e => e.Description)));
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to set PersonId on user {Email}", Input.Email);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Non-fatal: failed to create domain Person for {Email}", Input.Email);
                    }

                    // assign default role if role store is supported
                    if (_userManager.SupportsUserRole)
                    {
                        try
                        {
                            await _userManager.AddToRoleAsync(user, "client");
                        }
                        catch (NotSupportedException ex)
                        {
                            _logger.LogWarning(ex, "Role store not available; skipping role assignment for user {Email}", Input.Email);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Role store not available; skipping role assignment for user {Email}", Input.Email);
                    }

                    // sign-in immediately and redirect to returnUrl
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
