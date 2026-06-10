using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Area42_1.Web;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;
    private const string TokenKey = "auth_token";
    private const string AdminFlagKey = "is_admin";

    public CustomAuthStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await GetTokenFromStorageAsync();

            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = ParseClaimsFromToken(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var principal = new ClaimsPrincipal(identity);

            return new AuthenticationState(principal);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Auth state error: {ex.Message}");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public async Task LoginAsync(string token)
    {
        await SaveTokenToStorageAsync(token);

        // Parse token to check if admin
        var claims = ParseClaimsFromToken(token);
        var isAdmin = claims.Any(c => c.Type == "userType" && c.Value == "Admin") ||
                     claims.Any(c => c.Type == "isAdmin" && c.Value == "true");

        if (isAdmin)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", AdminFlagKey, "true");
        }

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task LogoutAsync()
    {
        await RemoveTokenFromStorageAsync();
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", AdminFlagKey);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private async Task<string?> GetTokenFromStorageAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TokenKey);
        }
        catch
        {
            return null;
        }
    }

    private async Task SaveTokenToStorageAsync(string token)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save token: {ex.Message}");
        }
    }

    private async Task RemoveTokenFromStorageAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        }
        catch
        {
            // Ignore removal failures
        }
    }

    private List<Claim> ParseClaimsFromToken(string token)
    {
        var claims = new List<Claim>();

        try
        {
            var parts = token.Split('.');
            if (parts.Length == 3)
            {
                var payload = parts[1];
                // Add padding if needed
                var padding = 4 - (payload.Length % 4);
                if (padding != 4)
                    payload += new string('=', padding);

                var decodedBytes = Convert.FromBase64String(payload);
                var json = System.Text.Encoding.UTF8.GetString(decodedBytes);

                // Use JsonDocument for safe parsing
                using (var doc = JsonDocument.Parse(json))
                {
                    foreach (var element in doc.RootElement.EnumerateObject())
                    {
                        var claimType = MapClaimType(element.Name);
                        var claimValue = element.Value.GetString() ?? element.Value.ToString();

                        if (!string.IsNullOrEmpty(claimValue))
                        {
                            claims.Add(new Claim(claimType, claimValue));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Token parsing error: {ex.Message}");
        }

        return claims;
    }

    private string MapClaimType(string jwtClaimName) => jwtClaimName switch
    {
        "sub" => ClaimTypes.NameIdentifier,
        "email" => ClaimTypes.Email,
        "role" => ClaimTypes.Role,
        "rank" => "rank",
        "name" => ClaimTypes.Name,
        "isAdmin" => "isAdmin",
        "userType" => "userType",
        _ => jwtClaimName
    };
}
