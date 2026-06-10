using Area42_1.Web.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Area42_1.Web.Services;

public class AccountService
{
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly ILogger<AccountService> _logger;
    private readonly string _dataFilePath;
    private readonly string _dataDirectory;
    private Dictionary<string, UserAccount> _accounts = new();

    public AccountService(IWebHostEnvironment hostEnvironment, ILogger<AccountService> logger)
    {
        _hostEnvironment = hostEnvironment;
        _logger = logger;
        _dataDirectory = Path.Combine(_hostEnvironment.WebRootPath, "..", "data");
        _dataFilePath = Path.Combine(_dataDirectory, "accounts.json");

        // Ensure data directory exists
        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
        }

        // Load existing accounts from file
        LoadAccountsFromFile();
    }

    public async Task<(bool success, string message, UserAccount? account)> RegisterAsync(UserRegistration registration)
    {
        // Validate email uniqueness
        if (_accounts.Values.Any(a => a.Email.Equals(registration.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return (false, "Email address already registered", null);
        }

        try
        {
            var account = new UserAccount
            {
                FirstName = registration.FirstName,
                LastName = registration.LastName,
                Tussenvoegsel = registration.Tussenvoegsel,
                DateOfBirth = registration.DateOfBirth,
                Street = registration.Street,
                HouseNumber = registration.HouseNumber,
                HouseLetter = registration.HouseLetter,
                PostalCode = registration.PostalCode,
                City = registration.City,
                Country = registration.Country,
                Gender = registration.Gender,
                Email = registration.Email,
                PhoneNumber = registration.PhoneNumber,
                PasswordHash = HashPassword(registration.Password),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _accounts[account.Id] = account;
            await SaveAccountsToFileAsync();

            _logger.LogInformation("Account registered successfully for email: {Email}", registration.Email);
            return (true, "Account registered successfully", account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering account for email: {Email}", registration.Email);
            return (false, "An error occurred while registering the account", null);
        }
    }

    public UserAccount? GetAccountByEmail(string email)
    {
        return _accounts.Values.FirstOrDefault(a => a.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public UserAccount? GetAccountById(string id)
    {
        return _accounts.TryGetValue(id, out var account) ? account : null;
    }

    public bool VerifyPassword(UserAccount account, string password)
    {
        return VerifyHash(password, account.PasswordHash);
    }

    public static string HashPassword(string password)
    {
        // Use PBKDF2 for password hashing
        const int iterations = 10000;
        const int saltSize = 32;
        const int hashSize = 32;

        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] salt = new byte[saltSize];
            rng.GetBytes(salt);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(hashSize);
                byte[] hashWithSalt = new byte[saltSize + hashSize];
                Array.Copy(salt, 0, hashWithSalt, 0, saltSize);
                Array.Copy(hash, 0, hashWithSalt, saltSize, hashSize);

                return Convert.ToBase64String(hashWithSalt);
            }
        }
    }

    public static bool VerifyHash(string password, string hash)
    {
        try
        {
            const int iterations = 10000;
            const int hashSize = 32;

            byte[] hashWithSalt = Convert.FromBase64String(hash);
            byte[] salt = new byte[hashWithSalt.Length - hashSize];
            Array.Copy(hashWithSalt, 0, salt, 0, salt.Length);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                byte[] computedHash = pbkdf2.GetBytes(hashSize);
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (hashWithSalt[salt.Length + i] != computedHash[i])
                        return false;
                }
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    private void LoadAccountsFromFile()
    {
        try
        {
            if (File.Exists(_dataFilePath))
            {
                var json = File.ReadAllText(_dataFilePath);
                var accounts = JsonSerializer.Deserialize<List<UserAccount>>(json) ?? new();
                _accounts = accounts.ToDictionary(a => a.Id);
                _logger.LogInformation("Loaded {Count} accounts from file", _accounts.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading accounts from file");
        }
    }

    private async Task SaveAccountsToFileAsync()
    {
        try
        {
            var accountsList = _accounts.Values.ToList();
            var json = JsonSerializer.Serialize(accountsList, new JsonSerializerOptions { WriteIndented = true });

            // Write to temp file first, then move (atomic operation)
            var tempPath = _dataFilePath + ".tmp";
            await File.WriteAllTextAsync(tempPath, json);

            // Backup existing file
            if (File.Exists(_dataFilePath))
            {
                var backupPath = _dataFilePath + ".bak";
                if (File.Exists(backupPath))
                    File.Delete(backupPath);
                File.Move(_dataFilePath, backupPath);
            }

            // Move temp to final
            File.Move(tempPath, _dataFilePath, overwrite: true);
            _logger.LogInformation("Saved {Count} accounts to file", _accounts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving accounts to file");
            throw;
        }
    }
}
