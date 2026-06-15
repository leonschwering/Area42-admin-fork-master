using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using QRCoder;

namespace Area42_1.Web.Backend;

public sealed class AdminTotpService(IWebHostEnvironment environment)
{
    private const string Issuer = "Area42";
    private const string AccountName = "admin1@area42.nl";
    private const int TimeStepSeconds = 30;
    private readonly string _statePath = Path.Combine(
        environment.ContentRootPath,
        "App_Data",
        "admin-authenticator.json");
    private readonly SemaphoreSlim _stateLock = new(1, 1);
    private readonly ConcurrentDictionary<string, LoginChallenge> _challenges = new();

    public async Task<AuthenticatorSetup> GetSetupAsync()
    {
        var state = await GetOrCreateStateAsync();
        var uri = BuildOtpAuthUri(state.Secret);
        var qrBytes = PngByteQRCodeHelper.GetQRCode(
            uri,
            QRCodeGenerator.ECCLevel.Q,
            size: 8);

        return new AuthenticatorSetup(
            state.Enabled,
            FormatSecret(state.Secret),
            $"data:image/png;base64,{Convert.ToBase64String(qrBytes)}");
    }

    public async Task<bool> IsEnabledAsync() => (await GetOrCreateStateAsync()).Enabled;

    public async Task<bool> VerifyAsync(string? code)
    {
        var state = await GetOrCreateStateAsync();
        return VerifyCode(state.Secret, code);
    }

    public async Task EnableAsync()
    {
        await _stateLock.WaitAsync();
        try
        {
            var state = await ReadStateAsync() ?? CreateState();
            if (!state.Enabled)
            {
                await WriteStateAsync(state with { Enabled = true });
            }
        }
        finally
        {
            _stateLock.Release();
        }
    }

    public string CreateChallenge(string returnUrl, bool setupRequired)
    {
        RemoveExpiredChallenges();
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        _challenges[token] = new LoginChallenge(
            returnUrl,
            setupRequired,
            DateTimeOffset.UtcNow.AddMinutes(5));
        return token;
    }

    public bool TryConsumeChallenge(string? token, out LoginChallenge challenge)
    {
        challenge = default!;
        if (string.IsNullOrWhiteSpace(token) ||
            !_challenges.TryRemove(token, out var stored) ||
            stored.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return false;
        }

        challenge = stored;
        return true;
    }

    public bool TryGetChallenge(string? token, out LoginChallenge challenge)
    {
        challenge = default!;
        if (string.IsNullOrWhiteSpace(token) ||
            !_challenges.TryGetValue(token, out var stored) ||
            stored.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return false;
        }

        challenge = stored;
        return true;
    }

    private async Task<AuthenticatorState> GetOrCreateStateAsync()
    {
        await _stateLock.WaitAsync();
        try
        {
            var state = await ReadStateAsync();
            if (state is not null)
            {
                return state;
            }

            state = CreateState();
            await WriteStateAsync(state);
            return state;
        }
        finally
        {
            _stateLock.Release();
        }
    }

    private async Task<AuthenticatorState?> ReadStateAsync()
    {
        if (!File.Exists(_statePath))
        {
            return null;
        }

        await using var stream = File.OpenRead(_statePath);
        return await JsonSerializer.DeserializeAsync<AuthenticatorState>(stream);
    }

    private async Task WriteStateAsync(AuthenticatorState state)
    {
        var directory = Path.GetDirectoryName(_statePath)!;
        Directory.CreateDirectory(directory);
        await using var stream = File.Create(_statePath);
        await JsonSerializer.SerializeAsync(
            stream,
            state,
            new JsonSerializerOptions { WriteIndented = true });
    }

    private static AuthenticatorState CreateState() =>
        new(ToBase32(RandomNumberGenerator.GetBytes(20)), false);

    private static bool VerifyCode(string secret, string? submittedCode)
    {
        var normalizedCode = new string(
            (submittedCode ?? string.Empty).Where(char.IsDigit).ToArray());
        if (normalizedCode.Length != 6)
        {
            return false;
        }

        var key = FromBase32(secret);
        var currentStep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / TimeStepSeconds;
        for (var offset = -1; offset <= 1; offset++)
        {
            var expectedCode = GenerateCode(key, currentStep + offset);
            if (CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(expectedCode),
                    Encoding.ASCII.GetBytes(normalizedCode)))
            {
                return true;
            }
        }

        return false;
    }

    private static string GenerateCode(byte[] key, long timeStep)
    {
        Span<byte> counter = stackalloc byte[8];
        for (var index = counter.Length - 1; index >= 0; index--)
        {
            counter[index] = (byte)(timeStep & 0xff);
            timeStep >>= 8;
        }

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(counter.ToArray());
        var offset = hash[^1] & 0x0f;
        var binaryCode =
            ((hash[offset] & 0x7f) << 24) |
            ((hash[offset + 1] & 0xff) << 16) |
            ((hash[offset + 2] & 0xff) << 8) |
            (hash[offset + 3] & 0xff);

        return (binaryCode % 1_000_000).ToString("D6", CultureInfo.InvariantCulture);
    }

    private static string BuildOtpAuthUri(string secret)
    {
        var label = Uri.EscapeDataString($"{Issuer}:{AccountName}");
        return $"otpauth://totp/{label}?secret={secret}&issuer={Uri.EscapeDataString(Issuer)}&digits=6&period={TimeStepSeconds}";
    }

    private static string FormatSecret(string secret) =>
        string.Join(' ', Enumerable.Range(0, (secret.Length + 3) / 4)
            .Select(index => secret.Substring(index * 4, Math.Min(4, secret.Length - index * 4))));

    private static string ToBase32(ReadOnlySpan<byte> data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new StringBuilder((data.Length * 8 + 4) / 5);
        var buffer = 0;
        var bitsLeft = 0;

        foreach (var value in data)
        {
            buffer = (buffer << 8) | value;
            bitsLeft += 8;
            while (bitsLeft >= 5)
            {
                bitsLeft -= 5;
                result.Append(alphabet[(buffer >> bitsLeft) & 31]);
            }
        }

        if (bitsLeft > 0)
        {
            result.Append(alphabet[(buffer << (5 - bitsLeft)) & 31]);
        }

        return result.ToString();
    }

    private static byte[] FromBase32(string value)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var output = new List<byte>(value.Length * 5 / 8);
        var buffer = 0;
        var bitsLeft = 0;

        foreach (var character in value.ToUpperInvariant())
        {
            var index = alphabet.IndexOf(character);
            if (index < 0)
            {
                continue;
            }

            buffer = (buffer << 5) | index;
            bitsLeft += 5;
            if (bitsLeft >= 8)
            {
                bitsLeft -= 8;
                output.Add((byte)(buffer >> bitsLeft));
                buffer &= (1 << bitsLeft) - 1;
            }
        }

        return output.ToArray();
    }

    private void RemoveExpiredChallenges()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var item in _challenges.Where(item => item.Value.ExpiresAt <= now))
        {
            _challenges.TryRemove(item.Key, out _);
        }
    }

    private sealed record AuthenticatorState(string Secret, bool Enabled);
}

public sealed record AuthenticatorSetup(bool Enabled, string ManualKey, string QrCodeDataUri);

public sealed record LoginChallenge(
    string ReturnUrl,
    bool SetupRequired,
    DateTimeOffset ExpiresAt);
