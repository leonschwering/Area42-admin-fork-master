namespace Area42_1.Web.Services;

using System.Text.Json;

public class AuthApiClient
{
    private readonly HttpClient _httpClient;

    public AuthApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Area42API");
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var request = new { email, password };
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LoginResponse>(content);
        }

        return null;
    }

    public async Task<RegisterResponse?> RegisterAsync(string email, string firstName, string lastName, string password)
    {
        var request = new { email, firstName, lastName, password, confirmPassword = password };
        var response = await _httpClient.PostAsJsonAsync("/api/auth/register", request);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<RegisterResponse>(content);
        }

        return null;
    }
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public UserDto? User { get; set; }
}

public class RegisterResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public UserDto? User { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
