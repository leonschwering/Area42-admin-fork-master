using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Area42_1.Web.Backend;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Area42_1.Web.Pages;

public sealed class AdminLoginModel(AdminTotpService totpService) : PageModel
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

    [BindProperty]
    public string ChallengeToken { get; set; } = string.Empty;

    [BindProperty]
    [Display(Name = "Authenticator-code")]
    public string AuthenticatorCode { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
    public bool IsAuthenticatorStep { get; private set; }
    public bool IsSetupStep { get; private set; }
    public string? QrCodeDataUri { get; private set; }
    public string? ManualKey { get; private set; }

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
            !CredentialsAreValid())
        {
            ErrorMessage = "De combinatie van e-mailadres en wachtwoord is niet juist.";
            return Page();
        }

        return await ShowAuthenticatorStepAsync();
    }

    public async Task<IActionResult> OnPostDemoAsync()
    {
        ModelState.Clear();
        Email = DemoEmail;
        Password = DemoPassword;
        return await ShowAuthenticatorStepAsync();
    }

    public async Task<IActionResult> OnPostAuthenticatorAsync()
    {
        ModelState.Clear();
        if (!totpService.TryGetChallenge(ChallengeToken, out var challenge))
        {
            ErrorMessage = "De inlogpoging is verlopen. Log opnieuw in.";
            Email = DemoEmail;
            Password = DemoPassword;
            return Page();
        }

        if (!await totpService.VerifyAsync(AuthenticatorCode))
        {
            ErrorMessage = "De Authenticator-code is niet juist of is verlopen.";
            await PopulateAuthenticatorStepAsync(challenge.SetupRequired);
            return Page();
        }

        if (!totpService.TryConsumeChallenge(ChallengeToken, out challenge))
        {
            ErrorMessage = "De inlogpoging is verlopen. Log opnieuw in.";
            return Page();
        }

        if (challenge.SetupRequired)
        {
            await totpService.EnableAsync();
        }

        return await SignInAdminAsync(challenge.ReturnUrl);
    }

    private bool CredentialsAreValid() =>
        string.Equals(Email.Trim(), DemoEmail, StringComparison.OrdinalIgnoreCase) &&
        Password.Trim() == DemoPassword;

    private async Task<IActionResult> ShowAuthenticatorStepAsync()
    {
        var setupRequired = !await totpService.IsEnabledAsync();
        ChallengeToken = totpService.CreateChallenge(GetSafeReturnUrl(ReturnUrl), setupRequired);
        await PopulateAuthenticatorStepAsync(setupRequired);
        return Page();
    }

    private async Task PopulateAuthenticatorStepAsync(bool setupRequired)
    {
        IsAuthenticatorStep = true;
        IsSetupStep = setupRequired;
        if (setupRequired)
        {
            var setup = await totpService.GetSetupAsync();
            QrCodeDataUri = setup.QrCodeDataUri;
            ManualKey = setup.ManualKey;
        }
    }

    private async Task<IActionResult> SignInAdminAsync(string returnUrl)
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

        return LocalRedirect(GetSafeReturnUrl(returnUrl));
    }

    private string GetSafeReturnUrl() => GetSafeReturnUrl(ReturnUrl);

    private string GetSafeReturnUrl(string? returnUrl) =>
        Url.IsLocalUrl(returnUrl) && returnUrl.StartsWith("/admin", StringComparison.OrdinalIgnoreCase)
            ? returnUrl
            : "/admin";
}
