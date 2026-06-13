using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Area42_1.Web.Pages;

public sealed class AdminLoginModel : PageModel
{
    private const string DemoEmail = "admin1@area42.nl";
    private const string DemoPassword = "Admin@123";

    [BindProperty]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string ReturnUrl { get; set; } = "/admin";

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(GetSafeReturnUrl());
        }

        Email = DemoEmail;
        Password = DemoPassword;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid ||
            !string.Equals(Email.Trim(), DemoEmail, StringComparison.OrdinalIgnoreCase) ||
            Password.Trim() != DemoPassword)
        {
            ErrorMessage = "De combinatie van e-mailadres en wachtwoord is niet juist.";
            return Page();
        }

        return await SignInAdminAsync();
    }

    public Task<IActionResult> OnPostDemoAsync()
    {
        ModelState.Clear();
        return SignInAdminAsync();
    }

    private async Task<IActionResult> SignInAdminAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Area42 Admin"),
            new Claim(ClaimTypes.Email, DemoEmail),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            });

        return LocalRedirect(GetSafeReturnUrl());
    }

    private string GetSafeReturnUrl() =>
        Url.IsLocalUrl(ReturnUrl) && ReturnUrl.StartsWith("/admin", StringComparison.OrdinalIgnoreCase)
            ? ReturnUrl
            : "/admin";
}
