namespace Area42_1.ApiService.Services;

using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Area42_1.ApiService.Data.Repositories;
using Area42_1.ApiService.Models.Users;
using Area42_1.ApiService.Models.Auth;
using Area42_1.ApiService.Models.Admin;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    string GenerateJwtToken(User user);
    string GenerateJwtTokenForAdmin(AdminUser user);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IAdminRepository _adminRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IAdminRepository adminRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _adminRepository = adminRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new AuthResponse { Success = false, Message = "Email and password are required." };
        }

        if (request.Password != request.ConfirmPassword)
        {
            return new AuthResponse { Success = false, Message = "Passwords do not match." };
        }

        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new AuthResponse { Success = false, Message = "Email already registered." };
        }

        // Create new user
        var user = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = HashPassword(request.Password),
            Role = UserRole.Customer,
            IsActive = true
        };

        var createdUser = await _userRepository.CreateAsync(user);

        return new AuthResponse
        {
            Success = true,
            Message = "Registration successful",
            Token = GenerateJwtToken(createdUser),
            User = new UserDto
            {
                Id = createdUser.Id.ToString(),
                Email = createdUser.Email,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,
                Role = createdUser.Role.ToString()
            }
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new AuthResponse { Success = false, Message = "Email and password are required." };
        }

        // Check if user is an admin (admin users must use @area42.nl email)
        if (request.Email.EndsWith("@area42.nl", StringComparison.OrdinalIgnoreCase))
        {
            return await LoginAdminAsync(request);
        }

        // Find regular customer user
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
        {
            return new AuthResponse { Success = false, Message = "Invalid email or password." };
        }

        // Verify password
        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            return new AuthResponse { Success = false, Message = "Invalid email or password." };
        }

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful",
            Token = GenerateJwtToken(user),
            User = new UserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString()
            }
        };
    }

    private async Task<AuthResponse> LoginAdminAsync(LoginRequest request)
    {
        // Find admin user
        var adminUser = await _adminRepository.GetByEmailAsync(request.Email);
        if (adminUser == null || !adminUser.IsEnabled || adminUser.IsLocked)
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Invalid credentials or account is disabled. Please contact an administrator." 
            };
        }

        // Verify password
        if (!VerifyPassword(request.Password, adminUser.PasswordHash))
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Invalid credentials. Please contact an administrator if you forgot your password." 
            };
        }

        // Update last login
        adminUser.LastLoginAt = DateTime.UtcNow;
        await _adminRepository.UpdateAsync(adminUser);

        return new AuthResponse
        {
            Success = true,
            Message = "Admin login successful",
            Token = GenerateJwtTokenForAdmin(adminUser),
            User = new UserDto
            {
                Id = adminUser.Id,
                Email = adminUser.Email,
                FirstName = adminUser.FullName.Split(' ').First(),
                LastName = adminUser.FullName.Contains(' ') ? string.Join(" ", adminUser.FullName.Split(' ').Skip(1)) : "",
                Role = adminUser.Rank?.ToString() ?? "Admin"
            },
            IsAdmin = true,
            UserRank = adminUser.Rank?.ToString()
        };
    }

    public string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? ""));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new System.Security.Claims.Claim("sub", user.Id.ToString()),
            new System.Security.Claims.Claim("email", user.Email),
            new System.Security.Claims.Claim("role", user.Role.ToString()),
            new System.Security.Claims.Claim("name", $"{user.FirstName} {user.LastName}")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateJwtTokenForAdmin(AdminUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? ""));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new System.Security.Claims.Claim("sub", user.Id),
            new System.Security.Claims.Claim("email", user.Email),
            new System.Security.Claims.Claim("rank", user.Rank?.ToString() ?? "Admin"),
            new System.Security.Claims.Claim("name", user.FullName),
            new System.Security.Claims.Claim("isAdmin", "true"),
            new System.Security.Claims.Claim("userType", "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private static bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput == hash;
    }
}
